using expensetracker2._0.DTO;
using expensetracker2._0.Models;

namespace ExpenseTracker.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetCategories();
        Task<bool> CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task<(List<ExpenseDto>, List<IncomeSourcesDto>)> GetExpensesAndIncomeSourcesByCategoryNameAsync(string categoryName);
        Task UpdateCategoryAsync(int categoryId, BudgetDto budgetDto);
        Task<bool> DeleteCategoryAsync(int categoryId);
     
    }
}
