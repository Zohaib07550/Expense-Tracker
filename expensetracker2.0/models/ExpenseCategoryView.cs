using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class ExpenseCategoryView
{
    public int ExpenseDetailId { get; set; }

    public int? DetailExpenseId { get; set; }

    public decimal? ExpenseDetailAmount { get; set; }

    public DateTime? ExpenseDetailDate { get; set; }

    public int ExpenseId { get; set; }

    public string? ExpenseDescription { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;
}
