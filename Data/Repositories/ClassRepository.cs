// File: SIMS_FPT/Data/Repositories/ClassRepository.cs
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Data.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _config;

        public ClassRepository(IWebHostEnvironment env)
        {
            // Dữ liệu sẽ lưu trong file classes.csv
            _filePath = Path.Combine(env.ContentRootPath, "CSV_DATA", "classes.csv");

            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };
        }

        // Helper đọc dữ liệu
        private List<ClassModel> ReadAll()
        {
            if (!File.Exists(_filePath)) return new List<ClassModel>();
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _config);
                return csv.GetRecords<ClassModel>().ToList();
            }
            catch
            {
                return new List<ClassModel>();
            }
        }

        // Helper ghi dữ liệu
        private void WriteAll(List<ClassModel> list)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _config);
            csv.WriteRecords(list);
        }

        // Chức năng: Xem danh sách
        public List<ClassModel> GetAll() => ReadAll();

        // Chức năng: Xem chi tiết
        public ClassModel GetById(string id) => ReadAll().FirstOrDefault(c => c.ClassId == id);

        // Chức năng: Tạo lớp
        public void Add(ClassModel m)
        {
            var list = ReadAll();
            // Kiểm tra trùng ID nếu cần
            if (!list.Any(x => x.ClassId == m.ClassId))
            {
                list.Add(m);
                WriteAll(list);
            }
        }

        // Chức năng: Sửa lớp
        public void Update(ClassModel m)
        {
            var list = ReadAll();
            var index = list.FindIndex(x => x.ClassId == m.ClassId);
            if (index != -1)
            {
                list[index] = m;
                WriteAll(list);
            }
        }

        // Chức năng: Xóa lớp
        public void Delete(string id)
        {
            var list = ReadAll();
            var item = list.FirstOrDefault(x => x.ClassId == id);
            if (item != null)
            {
                list.Remove(item);
                WriteAll(list);
            }
        }
    }
}