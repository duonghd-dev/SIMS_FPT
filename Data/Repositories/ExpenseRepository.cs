using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SIMS_FPT.Data.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly string _path;

        public ExpenseRepository()
        {
            // Đảm bảo thư mục tồn tại
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "CSV_DATA");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            _path = Path.Combine(folder, "expenses.csv");

            // Tạo header đúng chuẩn nếu file chưa có
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "expense_id,item_name,quantity,unit_price,total_amount,source,purchase_date,purchased_by");
            }
        }

        private List<ExpenseModel> Parse()
        {
            var list = new List<ExpenseModel>();
            if (!File.Exists(_path)) return list;

            string[] lines = File.ReadAllLines(_path);

            // Regex xử lý CSV chuẩn (bỏ qua dấu phẩy trong ngoặc kép nếu có)
            var pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

            // i = 1 để bỏ qua Header
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                var v = Regex.Split(lines[i], pattern);

                // CSV có 8 cột, nếu thiếu thì bỏ qua dòng lỗi
                if (v.Length < 8) continue;

                list.Add(new ExpenseModel
                {
                    ExpenseId = v[0],
                    ItemName = v[1],
                    Quantity = int.TryParse(v[2], out var q) ? q : 0,
                    UnitPrice = decimal.TryParse(v[3], out var u) ? u : 0,
                    TotalAmount = decimal.TryParse(v[4], out var t) ? t : 0,
                    Source = v[5],
                    PurchaseDate = DateTime.TryParse(v[6], out var d) ? d : DateTime.Now,
                    PurchasedBy = v[7]
                });
            }
            return list;
        }

        private string Format(ExpenseModel m)
        {
            // Logic nghiệp vụ: Tự động tính Tổng tiền trước khi lưu
            m.TotalAmount = m.Quantity * m.UnitPrice;

            // Format ngày tháng chuẩn yyyy-MM-dd để tránh lỗi khi đọc lại
            return $"{m.ExpenseId},{m.ItemName},{m.Quantity},{m.UnitPrice},{m.TotalAmount},{m.Source},{m.PurchaseDate:yyyy-MM-dd},{m.PurchasedBy}";
        }

        public List<ExpenseModel> GetAll() => Parse();

        public ExpenseModel GetById(string id) => Parse().FirstOrDefault(x => x.ExpenseId == id);

        public void Add(ExpenseModel m)
        {
            // Tự động sinh ID: EXP + số thứ tự (ví dụ EXP004)
            var all = GetAll();
            int nextId = 1;
            if (all.Any())
            {
                // Lấy số từ ID cuối cùng (Giả sử ID dạng EXPxxx)
                var lastId = all.Last().ExpenseId;
                if (lastId.Length > 3 && int.TryParse(lastId.Substring(3), out int idNum))
                {
                    nextId = idNum + 1;
                }
            }
            m.ExpenseId = "EXP" + nextId.ToString("D3"); // D3 nghĩa là 001, 002...

            File.AppendAllText(_path, Environment.NewLine + Format(m));
        }

        public void Update(ExpenseModel m)
        {
            var lines = File.ReadAllLines(_path).ToList();
            bool found = false;

            // Duyệt từ dòng 1 (sau header)
            for (int i = 1; i < lines.Count; i++)
            {
                var cols = lines[i].Split(',');
                if (cols.Length > 0 && cols[0] == m.ExpenseId)
                {
                    lines[i] = Format(m); // Ghi đè dòng cũ bằng dữ liệu mới
                    found = true;
                    break;
                }
            }

            if (found) File.WriteAllLines(_path, lines);
        }

        public void Delete(string id)
        {
            var lines = File.ReadAllLines(_path).ToList();
            // Giữ lại Header và các dòng KHÔNG trùng ID
            var newLines = lines.Where((line, index) => index == 0 || line.Split(',')[0] != id).ToList();

            File.WriteAllLines(_path, newLines);
        }
    }
}