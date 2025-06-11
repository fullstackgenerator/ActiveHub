using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ActiveHub.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "First Name")]
    [PersonalData]
    public string FirstName { get; set; }
    
    [Required]
    [Display(Name = "Last Name")]
    [PersonalData]
    public string LastName { get; set; }
    
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    [PersonalData]
    public DateOnly? DateOfBirth { get; set; }
    
    [PersonalData]
    public string? Address { get; set; }
    
    [Display(Name = "Postal Code")]
    [PersonalData]
    public int? PostNumber { get; set; }
    
    [PersonalData]
    public string? City { get; set; }
    
    ICollection<Membership> Memberships { get; set; }
}