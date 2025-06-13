using System;
using System.ComponentModel.DataAnnotations;

namespace ActiveHub.Models;

public class EditUserProfile
{
    public string Id { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [Display(Name = "Postal Code")]
    public int? PostNumber { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }
}