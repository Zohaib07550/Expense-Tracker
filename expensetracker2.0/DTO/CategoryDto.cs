using System.Collections.Generic; 
using expensetracker2._0.DTO; 

namespace expensetracker2._0.DTO
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public ICollection<ExpenseDto> Expenses { get; set; }
        public ICollection<IncomeSourcesDto> IncomeSources { get; set; }
        public ICollection<BudgetDto> Budgets { get; set; }
    }
}
