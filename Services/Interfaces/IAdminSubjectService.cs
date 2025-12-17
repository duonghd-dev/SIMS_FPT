using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Services.Interfaces
{
    public interface IAdminSubjectService
    {
        List<SubjectModel> GetAllSubjects();
        SubjectModel? GetSubjectById(string id);
        Dictionary<string, string> GetTeacherNamesByIds();
        (bool Success, string Message) AddSubject(SubjectModel model);
        (bool Success, string Message) UpdateSubject(SubjectModel model);
        (bool Success, string Message) DeleteSubject(string id);
    }
}
