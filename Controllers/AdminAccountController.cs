using ActiveHub.Data;
using ActiveHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq; // Ensure this is present for LINQ methods
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActiveHub.Controllers;

public class AdminAccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminAccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLogin model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result =
            await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Only administrators are allowed to log in here.");
                return View(model);
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "AdminAccount");
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Today;
        var endOfWeek = today.AddDays(7);

        var totalUsers = await _userManager.Users.CountAsync();

        var newUsersTodayCount = await _userManager.Users
            .CountAsync(u => u.RegistrationDate.HasValue && u.RegistrationDate.Value.Date == today.Date);

        var activeMemberships = await _context.Memberships
            .CountAsync(m => m.StartDate <= today && m.EndDate >= today);

        var expiringMembershipsThisWeek = await _context.Memberships
            .CountAsync(m => m.EndDate >= today && m.EndDate <= endOfWeek);

        var utcNow = DateTimeOffset.UtcNow;

        var lockedOutUsers = await _userManager.Users
            .Where(u => u.LockoutEnd.HasValue)
            .ToListAsync();

        var lockedOutUsersCount = lockedOutUsers
            .Count(u => u.LockoutEnd > utcNow);

        var viewModel = new AdminDashboard
        {
            TotalUsers = totalUsers,
            NewUsersToday = newUsersTodayCount,
            ActiveMemberships = activeMemberships,
            ExpiringMembershipsThisWeek = expiringMembershipsThisWeek,
            LockedOutUsers = lockedOutUsersCount
        };

        return View(viewModel);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StatsAndData(string? fromDate, string? toDate)
    {
        var model = new StatsViewModel
        {
            FromDate = fromDate,
            ToDate = toDate,
            DashboardStats = new AdminDashboard()
        };

        DateTime? parsedFromDate = null;
        DateTime? parsedToDate = null;

        if (DateTime.TryParse(fromDate, out var parsedDate))
        {
            parsedFromDate = parsedDate.Date;
        }

        if (DateTime.TryParse(toDate, out parsedDate))
        {
            parsedToDate = parsedDate.Date.AddDays(1).AddTicks(-1); // End of the day
        }

        //populate UserRegistrations
        //filtering by date range if provided
        IQueryable<ApplicationUser> userQuery = _context.ApplicationUsers;
        if (parsedFromDate.HasValue)
        {
            userQuery = userQuery.Where(u => u.RegistrationDate.HasValue && u.RegistrationDate.Value.Date >= parsedFromDate.Value);
        }
        if (parsedToDate.HasValue)
        {
            userQuery = userQuery.Where(u => u.RegistrationDate.HasValue && u.RegistrationDate.Value.Date <= parsedToDate.Value);
        }

        model.UserRegistrations = await userQuery
            .Where(u => u.RegistrationDate.HasValue)
            .GroupBy(u => u.RegistrationDate.Value.Date)
            .Select(g => new UserRegistrationStat { Date = g.Key, Count = g.Count() })
            .OrderBy(s => s.Date)
            .ToListAsync();

        //populate ProgramUsage based on MembershipType and Memberships
        //counts how many memberships of each special program type exist
        IQueryable<Membership> programUsageQuery = _context.Memberships
            .Include(m => m.MembershipType)
            .Where(m => m.MembershipType.Category == MembershipCategory.Special);

        if (parsedFromDate.HasValue)
        {
            programUsageQuery = programUsageQuery.Where(m => m.StartDate >= parsedFromDate.Value);
        }
        if (parsedToDate.HasValue)
        {
            programUsageQuery = programUsageQuery.Where(m => m.StartDate <= parsedToDate.Value);
        }

        model.ProgramUsage = await programUsageQuery
            .GroupBy(m => m.MembershipType.Name)
            .Select(g => new ProgramUsageStat { ProgramName = g.Key, UsageCount = g.Count() })
            .OrderBy(s => s.ProgramName)
            .ToListAsync();


        //populate IncomeStats based on Memberships and MembershipTypes
        //calculates income from memberships by their start date
        IQueryable<Membership> incomeQuery = _context.Memberships
            .Include(m => m.MembershipType); // Include MembershipType to access price

        if (parsedFromDate.HasValue)
        {
            incomeQuery = incomeQuery.Where(m => m.StartDate >= parsedFromDate.Value);
        }
        if (parsedToDate.HasValue)
        {
            incomeQuery = incomeQuery.Where(m => m.StartDate <= parsedToDate.Value);
        }

        model.IncomeStats = await incomeQuery
            .GroupBy(m => m.StartDate.Date)
            .Select(g => new IncomeStat { Date = g.Key, Amount = g.Sum(m => m.MembershipType.Price) })
            .OrderBy(s => s.Date)
            .ToListAsync();

        return View("Stats", model);
    }
    
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Dashboard");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}