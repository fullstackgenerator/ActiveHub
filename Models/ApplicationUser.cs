using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ActiveHub.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "First Name")]
    [PersonalData]
    public required string FirstName { get; set; }
    
    [Required]
    [Display(Name = "Last Name")]
    [PersonalData]
    public required string LastName { get; set; }
    
    [Display(Name = "Registration Date")]
    [DataType(DataType.DateTime)]
    [PersonalData]
    public DateTime? RegistrationDate { get; set; }
    
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    [PersonalData]
    public DateOnly? DateOfBirth { get; set; }
    
    [PersonalData]
    [MaxLength(250)]
    public string? Address { get; set; }
    
    [Display(Name = "Postal Code")]
    [PersonalData]
    public int? PostNumber { get; set; }
    
    [PersonalData]
    [MaxLength(250)]
    public string? City { get; set; }
    
    ICollection<Membership> Memberships { get; set; }
}