using System.Diagnostics;
using ActiveHub.Data;
using Microsoft.AspNetCore.Mvc;
using ActiveHub.Models;
using Microsoft.EntityFrameworkCore;

namespace ActiveHub.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet("/program")]
    public async Task<IActionResult> Program()
    {
        //fetch all active membership types from DB
        var membershipTypes = await _context.MembershipTypes
            .Where(mt => mt.IsActive)
            .ToListAsync();

        //pass list of MembershipTypes to the view
        return View("~/Views/Program/Index.cshtml", membershipTypes);
    }
}