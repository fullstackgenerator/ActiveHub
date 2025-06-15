using ActiveHub.Data;
using ActiveHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public IActionResult Login(string? returnUrl = null) //for redirecting after login
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

        //find user by email first
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        //sign the user in
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

            if (Url.IsLocalUrl(returnUrl)) //validate return urls for security
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "AdminAccount");
        }
        else if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
        }
        else //like wrong password
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
        var today = DateTime.Today; // Only date part
        var endOfWeek = today.AddDays(7);

        //fetch the data dynamically
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
    public IActionResult StatsAndData(string? fromDate, string? toDate)
    {
        ViewData["fromDate"] = fromDate;
        ViewData["toDate"] = toDate;

        return View("Stats");
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