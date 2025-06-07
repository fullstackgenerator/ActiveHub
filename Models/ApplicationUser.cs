using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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
    
    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    [PersonalData]
    public DateOnly DateOfBirth { get; set; }
    
    [Required]
    [PersonalData]
    public string Address { get; set; }
    
    [Required]
    [Display(Name = "Postal Code")]
    [PersonalData]
    public int PostNumber { get; set; }
    
    [Required]
    [PersonalData]
    public string City { get; set; }
}