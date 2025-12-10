using CsvHelper.Configuration.Attributes;

namespace SIMS_FPT.Models
{
    public class SubjectModel
    {
        [Name("subject_id")]
        public string SubjectId { get; set; }

        [Name("subject_name")]
        public string SubjectName { get; set; }

        [Name("department_id")]
        public string DepartmentId { get; set; }

        [Name("credits")]
        public int Credits { get; set; }
    }
}