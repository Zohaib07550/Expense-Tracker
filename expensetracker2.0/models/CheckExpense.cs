using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class CheckExpense
{
    public int? ExpenseId { get; set; }

    public decimal? Amount { get; set; }
}
