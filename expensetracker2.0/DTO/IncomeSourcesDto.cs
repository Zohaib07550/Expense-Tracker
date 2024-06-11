namespace expensetracker2._0.DTO
{
    public class IncomeSourcesDto
    {
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public List<IncomeEntryDto> IncomeEntry { get; set; }

    }
}
