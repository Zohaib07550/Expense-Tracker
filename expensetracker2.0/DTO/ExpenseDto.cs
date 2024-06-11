using expensetracker2._0.DTO; // Import the namespace where ExpenseDTO is defined

namespace expensetracker2._0.DTO
{
    public class ExpenseDto
    {
        public string Description { get; set; }
        public int CategoryId { get; set; } // Add this property
        public List<ExpenseDetailDto> ExpenseDetails { get; set; }
    }
}
