using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ActiveHub.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }
    
    public string Address { get; set; }
    
    [Display(Name = "Postal Code")]
    public int PostNumber { get; set; }
    
    public string City { get; set; }
}