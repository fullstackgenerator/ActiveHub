using System.ComponentModel.DataAnnotations;

namespace ActiveHub.Models;

public class MembershipType
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } // yearly membership, special 1, special 2,...
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public MembershipCategory Category { get; set; } // regular or special category
    
    public int? DurationInDays { get; set; } // special programs duration
    
    public int? MaxParticipants { get; set; } // for special programs only
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}

public enum MembershipCategory
{
    Regular,
    Special
}