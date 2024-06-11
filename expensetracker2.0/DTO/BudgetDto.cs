namespace expensetracker2._0.DTO
{
    public class BudgetDto
    {
        public int? CategoryId { get; set; }

        public decimal? Budget1 { get; set; }

        public string Interval { get; set; } = null!;
        public string? Description { get; set; }
    }
}
