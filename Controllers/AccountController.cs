using ActiveHub.Data;
using ActiveHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActiveHub.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
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
    public async Task<IActionResult> Login(Login model, string? returnUrl = null)
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
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "Administrators must log in via the admin portal.");
                return View(model);
            }

            if (Url.IsLocalUrl(returnUrl)) //validate return urls for security
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Account");
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
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        var userMembership = await _context.Memberships
            .Where(m => m.UserId == userId && m.EndDate >= DateTime.UtcNow)
            .Include(m => m.MembershipType)
            .ToListAsync();

        return View(userMembership);
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

            TempData["PasswordChangeSuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Dashboard");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData["PasswordChangeErrorMessage"] = "Failed to change password.";
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        var viewModel = new EditUserProfile
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            PostNumber = user.PostNumber,
            City = user.City
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserProfile model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;
        user.DateOfBirth = model.DateOfBirth;
        user.Address = model.Address;
        user.PostNumber = model.PostNumber;
        user.City = model.City;

        var updateResult = await _userManager.UpdateAsync(user);

        if (updateResult.Succeeded)
        {
            TempData["ProfileEditSuccessMessage"] = "Your profile has been updated successfully.";
            return RedirectToAction("Dashboard");
        }
        else
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, $"Error updating profile: {error.Description}");
            }

            return View(model);
        }
    }
}