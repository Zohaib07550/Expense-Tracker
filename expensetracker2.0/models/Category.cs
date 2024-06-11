using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<IncomeSource> IncomeSources { get; set; } = new List<IncomeSource>();
}
