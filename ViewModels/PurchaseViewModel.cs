using ActiveHub.Models;
using System.Collections.Generic;

namespace ActiveHub.ViewModels;

public class PurchaseViewModel
{
    public MembershipType MembershipType { get; set; }
    public List<PaymentMethod> PaymentMethods { get; set; }
    
    public DateTime StartDate { get; set; }
}