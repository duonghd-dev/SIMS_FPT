// File: SIMS_FPT/Data/Interfaces/IClassSubjectRepository.cs
using SIMS_FPT.Models;

namespace SIMS_FPT.Data.Interfaces
{
    public interface IClassSubjectRepository
    {
        List<ClassSubjectModel> GetAll();
        List<ClassSubjectModel> GetByClassId(string classId);
        List<ClassSubjectModel> GetBySubjectId(string subjectId);
        List<ClassSubjectModel> GetByTeacherId(string teacherId);
        ClassSubjectModel? GetByIds(string classId, string subjectId);
        void Add(ClassSubjectModel model);
        void Update(ClassSubjectModel model);
        void Delete(string classId, string subjectId);
        void DeleteByClassId(string classId);
        bool Exists(string classId, string subjectId);
    }
}
