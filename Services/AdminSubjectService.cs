using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using SIMS_FPT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class AdminSubjectService : IAdminSubjectService
    {
        private readonly ISubjectRepository _subjectRepo;
        private readonly IDepartmentRepository _deptRepo;
        private readonly ITeacherRepository _teacherRepo;

        public AdminSubjectService(ISubjectRepository subjectRepo, IDepartmentRepository deptRepo, ITeacherRepository teacherRepo)
        {
            _subjectRepo = subjectRepo;
            _deptRepo = deptRepo;
            _teacherRepo = teacherRepo;
        }

        public List<SubjectModel> GetAllSubjects() => _subjectRepo.GetAll();

        public SubjectModel? GetSubjectById(string id) => _subjectRepo.GetById(id);

        public Dictionary<string, string> GetTeacherNamesByIds()
        {
            return _teacherRepo.GetAll().ToDictionary(t => t.TeacherId, t => t.Name);
        }

        public (bool Success, string Message) AddSubject(SubjectModel model)
        {
            try
            {
                if (model == null)
                    return (false, "Subject model cannot be null");

                if (string.IsNullOrWhiteSpace(model.SubjectId))
                    return (false, "Subject ID is required");

                if (string.IsNullOrWhiteSpace(model.SubjectName))
                    return (false, "Subject Name is required");

                if (model.Credits < 1 || model.Credits > 10)
                    return (false, "Credits must be between 1 and 10");

                // Check duplicate
                if (_subjectRepo.GetById(model.SubjectId) != null)
                    return (false, "Subject ID already exists");

                _subjectRepo.Add(model);
                return (true, "Subject added successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error adding subject: {ex.Message}");
            }
        }

        public (bool Success, string Message) UpdateSubject(SubjectModel model)
        {
            try
            {
                if (model == null)
                    return (false, "Subject model cannot be null");

                if (string.IsNullOrWhiteSpace(model.SubjectId))
                    return (false, "Subject ID is required");

                if (model.Credits < 1 || model.Credits > 10)
                    return (false, "Credits must be between 1 and 10");

                _subjectRepo.Update(model);
                return (true, "Subject updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating subject: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteSubject(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return (false, "Subject ID cannot be empty");

                _subjectRepo.Delete(id);
                return (true, "Subject deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting subject: {ex.Message}");
            }
        }
    }
}
