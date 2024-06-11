using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class IncomeSource
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? Date { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<IncomeEntry> IncomeEntries { get; set; } = new List<IncomeEntry>();
}
