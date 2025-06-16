using ActiveHub.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using ActiveHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActiveHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExportController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ExportController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ExportStatsToExcel(string? fromDate, string? toDate)
        {
            DateTime? parsedFromDate = null;
            DateTime? parsedToDate = null;

            if (DateTime.TryParse(fromDate, out DateTime tempFromDate))
            {
                parsedFromDate = tempFromDate.Date;
            }

            if (DateTime.TryParse(toDate, out DateTime tempToDate))
            {
                parsedToDate = tempToDate.Date.AddDays(1).AddSeconds(-1);
            }

            //create a workbook
            var workbook = new XLWorkbook();

            try
            {
                //add user registrations worksheet
                var userRegistrations = await GetUserRegistrations(parsedFromDate, parsedToDate);
                AddWorksheet(workbook, userRegistrations, "User Registrations");
                
                var programUsage = await GetProgramUsage(parsedFromDate, parsedToDate);
                AddWorksheet(workbook, programUsage, "Program Usage");
                
                var incomeStats = await GetIncomeStats(parsedFromDate, parsedToDate);
                AddWorksheet(workbook, incomeStats, "Income");
                
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"FitPro_{DateTime.Now:ddMMyyyy}.xlsx");
            }
            finally
            {
                workbook.Dispose();
            }
        }

        private async Task<List<UserRegistrationStat>> GetUserRegistrations(DateTime? fromDate, DateTime? toDate)
        {
            return await _userManager.Users
                .Where(u => (!fromDate.HasValue || u.RegistrationDate >= fromDate) &&
                            (!toDate.HasValue || u.RegistrationDate <= toDate))
                .GroupBy(u => u.RegistrationDate.HasValue ? u.RegistrationDate.Value.Date : DateTime.MinValue.Date)
                .Select(g => new UserRegistrationStat
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        private async Task<List<ProgramUsageStat>> GetProgramUsage(DateTime? fromDate, DateTime? toDate)
        {
            return await _context.Memberships
                .Include(m => m.MembershipType)
                .Where(m => (!fromDate.HasValue || m.PurchaseDate >= fromDate) &&
                            (!toDate.HasValue || m.PurchaseDate <= toDate))
                .GroupBy(m => m.MembershipType.Name)
                .Select(g => new ProgramUsageStat
                {
                    ProgramName = g.Key,
                    UsageCount = g.Count()
                })
                .ToListAsync();
        }

        private async Task<List<IncomeStat>> GetIncomeStats(DateTime? fromDate, DateTime? toDate)
        {
            return await _context.Memberships
                .Include(m => m.MembershipType)
                .Where(m => (!fromDate.HasValue || m.PurchaseDate >= fromDate) &&
                            (!toDate.HasValue || m.PurchaseDate <= toDate))
                .GroupBy(m => m.PurchaseDate.Date)
                .Select(g => new IncomeStat
                {
                    Date = g.Key,
                    Amount = g.Sum(m => m.MembershipType.Price)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        private void AddWorksheet<T>(XLWorkbook workbook, List<T> data, string sheetName) where T : class
        {
            var worksheet = workbook.Worksheets.Add(sheetName);
            
            var table = new DataTable();
            var properties = typeof(T).GetProperties();

            // Add columns
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, prop.PropertyType);
            }

            //add rows
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            //add data to worksheet
            worksheet.Cell(1, 1).InsertTable(table);

            //headers
            var header = worksheet.Range(1, 1, 1, properties.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            //format currency columns
            if (sheetName == "Income")
            {
                // Find the "Amount" column
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].Name == "Amount" && properties[i].PropertyType == typeof(decimal))
                    {
                        var amountColumn = worksheet.Column(i + 1);
                        amountColumn.Style.NumberFormat.Format = "€#,##0.00";
                        break;
                    }
                }
            }
            
            worksheet.Columns().AdjustToContents();
        }
    }
}