using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class ExpenseDetail
{
    public int ExpenseDetailId { get; set; }

    public int? ExpenseId { get; set; }

    public decimal? Amount { get; set; }

    public string? Description { get; set; }

    public DateTime? Date { get; set; }

    public virtual Expense? Expense { get; set; }
}
