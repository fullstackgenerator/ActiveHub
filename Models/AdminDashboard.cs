namespace ActiveHub.Models;

public class AdminDashboard
{
    public int TotalUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int ActiveMemberships { get; set; }
    public int ExpiringMembershipsThisWeek { get; set; }
    public int LockedOutUsers { get; set; }
}