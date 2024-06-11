using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class Budget
{
    public int BudgetId { get; set; }

    public int? CategoryId { get; set; }

    public decimal? Budget1 { get; set; }

    public string Interval { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Category? Category { get; set; }
}
