using ActiveHub.Data;
using ActiveHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActiveHub.ViewModels;

namespace ActiveHub.Controllers;

public class PricingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PricingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    //goes from Home/Program page (Enroll Now button)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiatePurchase(int membershipTypeId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var selectedMembershipType = await _context.MembershipTypes
                                                    .FirstOrDefaultAsync(mt => mt.Id == membershipTypeId && mt.IsActive);

        if (selectedMembershipType == null)
        {
            TempData["ErrorMessage"] = "Selected program not found or is inactive.";
            return RedirectToAction("Program", "Home");
        }

        TempData["SelectedMembershipTypeId"] = membershipTypeId;

        return RedirectToAction("Purchase");
    }

    //purchase confirmation page
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Purchase()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (!TempData.TryGetValue("SelectedMembershipTypeId", out object? selectedIdObj) || !(selectedIdObj is int selectedMembershipTypeId))
        {
            TempData["ErrorMessage"] = "No program selected for purchase. Please choose a program.";
            return RedirectToAction("Program", "Home");
        }

        var selectedMembershipType = await _context.MembershipTypes
                                                    .FirstOrDefaultAsync(mt => mt.Id == selectedMembershipTypeId && mt.IsActive);

        if (selectedMembershipType == null)
        {
            TempData["ErrorMessage"] = "Selected program not found or is inactive.";
            return RedirectToAction("Program", "Home");
        }

        var model = new PurchaseViewModel
        {
            MembershipType = selectedMembershipType,
            PaymentMethods = Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>().ToList(),
            StartDate = DateTime.Today
        };

        return View(model);
    }


    //enroll action is accepting the chosen startDate (POST)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int membershipTypeId, PaymentMethod paymentMethod, DateTime startDate)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var selectedMembershipType = await _context.MembershipTypes
                                                    .FirstOrDefaultAsync(mt => mt.Id == membershipTypeId && mt.IsActive);

        if (selectedMembershipType == null)
        {
            TempData["ErrorMessage"] = "Selected program not found or is inactive.";
            return RedirectToAction("Program", "Home");
        }

        //validate the selected start date
        if (startDate < DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue))
        {
            TempData["ErrorMessage"] = "Start date cannot be in the past.";
            TempData["SelectedMembershipTypeId"] = membershipTypeId;
            return RedirectToAction("Purchase");
        }

        var existingActiveMembership = await _context.Memberships
            .AnyAsync(m => m.UserId == userId &&
                           m.MembershipTypeId == membershipTypeId &&
                           m.StartDate <= DateTime.Today && // Membership has started
                           m.EndDate >= DateTime.Today);

        if (existingActiveMembership)
        {
            TempData["InfoMessage"] = $"You already have an active '{selectedMembershipType.Name}' membership.";
            return RedirectToAction("Dashboard", "Account");
        }

        DateTime endDate;
        if (selectedMembershipType.DurationInDays.HasValue && selectedMembershipType.DurationInDays.Value > 0)
        {
            endDate = startDate.AddDays(selectedMembershipType.DurationInDays.Value);
        }
        else
        {
            endDate = startDate.AddYears(1);
        }

        var newMembership = new Membership
        {
            UserId = userId,
            MembershipTypeId = selectedMembershipType.Id,
            PurchaseDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            PaymentMethod = paymentMethod
        };

        _context.Memberships.Add(newMembership);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Successfully enrolled in {selectedMembershipType.Name}! Your membership is active from {newMembership.StartDate:d} until {newMembership.EndDate:d}.";
        return RedirectToAction("Dashboard", "Account");
    }
}