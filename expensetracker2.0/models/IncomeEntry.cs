using System;
using System.Collections.Generic;

namespace expensetracker2._0.Models;

public partial class IncomeEntry
{
    public int IncomeEntryId { get; set; }

    public int IncomeSourceId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? Date { get; set; }

    public virtual IncomeSource IncomeSource { get; set; } = null!;
}
