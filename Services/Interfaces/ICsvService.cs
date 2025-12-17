using System.Collections.Generic;

namespace SIMS_FPT.Services.Interfaces
{
    public interface ICsvService
    {
        /// <summary>
        /// Reads CSV file and deserializes to list of T
        /// </summary>
        List<T> ReadCsv<T>(string filePath);
    }
}
