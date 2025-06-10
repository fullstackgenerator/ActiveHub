using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActiveHub.Models;

public class Membership
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }
    
    [Required]
    public int MembershipTypeId { get; set; }
    
    [ForeignKey("MembershipTypeId")]
    public MembershipType MembershipType { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Purchase Date")]
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }
    
    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }
    
    [NotMapped]
    public bool IsActive => DateTime.Today >= StartDate && DateTime.Today <= EndDate;
}

public enum PaymentMethod
{
    Banktransfer,
    CreditCard,
    DebitCard
}