using expensetracker2._0.DTO;
using expensetracker2._0.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace expensetracker2._0.Interface
{
    public interface IIncomeSources
    {
        Task<IncomeSource> GetIncomeSourceByIdAsync(int id);
        Task<List<IncomeSourcesDto>> GetIncomeSourcesAndEntriesByCategoryNameAsync(string categoryName);
        Task<IActionResult> AddOrCreateIncomeSourceAsync(IncomeSourcesDto incomeSourcesDto);
        void UpdateIncomeSource(int id, IncomeSourcesDto incomeSourceDto);
        public IEnumerable<IncomeSource> SearchIncomeSource(string query);
        IEnumerable<IncomeSource> FilterIncomeSource(int category, DateTime startDate, DateTime endDate);
        Task<bool> DeleteIncomeSourceAsync(int id);
    }
}
