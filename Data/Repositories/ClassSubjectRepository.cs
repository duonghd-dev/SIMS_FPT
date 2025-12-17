// File: SIMS_FPT/Data/Repositories/ClassSubjectRepository.cs
using CsvHelper;
using CsvHelper.Configuration;
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Models;
using System;
using System.Globalization;

namespace SIMS_FPT.Data.Repositories
{
    public class ClassSubjectRepository : IClassSubjectRepository
    {
        private readonly string _filePath = "CSV_DATA/class_subjects.csv";
        private readonly CsvConfiguration _csvConfig;

        public ClassSubjectRepository()
        {
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };
        }

        public List<ClassSubjectModel> GetAll()
        {
            if (!File.Exists(_filePath))
                return new List<ClassSubjectModel>();

            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, _csvConfig);
            return csv.GetRecords<ClassSubjectModel>().ToList();
        }

        public List<ClassSubjectModel> GetByClassId(string classId)
        {
            return GetAll().Where(cs => cs.ClassId.Equals(classId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<ClassSubjectModel> GetBySubjectId(string subjectId)
        {
            return GetAll().Where(cs => cs.SubjectId.Equals(subjectId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<ClassSubjectModel> GetByTeacherId(string teacherId)
        {
            return GetAll().Where(cs => cs.TeacherId.Equals(teacherId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public ClassSubjectModel? GetByIds(string classId, string subjectId)
        {
            return GetAll().FirstOrDefault(cs => cs.ClassId == classId && cs.SubjectId == subjectId);
        }

        public void Add(ClassSubjectModel model)
        {
            var records = GetAll();
            records.Add(model);
            SaveAll(records);
        }

        public void Update(ClassSubjectModel model)
        {
            var records = GetAll();
            var existing = records.FirstOrDefault(cs => cs.ClassId == model.ClassId && cs.SubjectId == model.SubjectId);
            if (existing != null)
            {
                existing.TeacherId = model.TeacherId;
                SaveAll(records);
            }
        }

        public void Delete(string classId, string subjectId)
        {
            var records = GetAll();
            records.RemoveAll(cs => cs.ClassId == classId && cs.SubjectId == subjectId);
            SaveAll(records);
        }

        public void DeleteByClassId(string classId)
        {
            var records = GetAll();
            records.RemoveAll(cs => cs.ClassId == classId);
            SaveAll(records);
        }

        public bool Exists(string classId, string subjectId)
        {
            return GetAll().Any(cs => cs.ClassId == classId && cs.SubjectId == subjectId);
        }

        private void SaveAll(List<ClassSubjectModel> records)
        {
            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, _csvConfig);
            csv.WriteRecords(records);
        }
    }
}
