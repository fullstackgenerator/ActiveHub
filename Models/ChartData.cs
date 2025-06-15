namespace ActiveHub.Models
{
    public class ChartDataPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }
    }

    public class StatsData
    {
        public List<ChartDataPoint> NewUsersByDate { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> ProgramsUsage { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> IncomeByDate { get; set; } = new List<ChartDataPoint>();
        public decimal TotalIncome { get; set; }
        public int TotalNewUsers { get; set; }
    }
}