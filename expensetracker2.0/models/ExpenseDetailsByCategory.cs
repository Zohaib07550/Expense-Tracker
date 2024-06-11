namespace expensetracker2._0.models
{
    public class ExpenseDetailsByCategory
    {
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
// modelBuilder.Entity<ExpenseDetailsByCategory>().HasNoKey().ToView("ExpenseDetailsByCategory");

//public DbSet<ExpenseDetailsByCategory> ExpenseDetailsByCategory { get; set; }