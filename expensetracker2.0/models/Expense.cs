using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class Expense
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ExpenseDetail> ExpenseDetails { get; set; } = new List<ExpenseDetail>();
}
