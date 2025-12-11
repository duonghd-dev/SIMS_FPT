using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_FPT.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(100)]
        public string ClassName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học")]
        public string SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public virtual SubjectModel? Subject { get; set; } // Thêm dấu ? để cho phép null

        [Required(ErrorMessage = "Vui lòng chọn giảng viên")]
        public string TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Users? Teacher { get; set; } // Thêm dấu ? để cho phép null

        [StringLength(50)]
        public string Semester { get; set; } 

        public virtual ICollection<Enrollment>? Enrollments { get; set; } // Thêm dấu ?
    }
}