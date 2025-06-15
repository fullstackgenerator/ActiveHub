using System;
using System.Collections.Generic;

namespace ActiveHub.Models
{
    public class StatsViewModel
    {
        public AdminDashboard DashboardStats { get; set; }
        public List<UserRegistrationStat> UserRegistrations { get; set; } = new();
        public List<ProgramUsageStat> ProgramUsage { get; set; } = new();
        public List<IncomeStat> IncomeStats { get; set; } = new();
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }

    public class UserRegistrationStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class ProgramUsageStat
    {
        public string ProgramName { get; set; }
        public int UsageCount { get; set; }
    }

    public class IncomeStat
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}