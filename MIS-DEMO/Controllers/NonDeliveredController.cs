using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using MIS_DEMO.Models.ViewModels;
using MIS_DEMO.Services;

namespace MIS_DEMO.Controllers
{
    public class NonDeliveredController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SalesAccessService _salesAccessService;
        private readonly IDateProvider _dateProvider;

        public NonDeliveredController(
            AppDbContext context,
            SalesAccessService salesAccessService,
            IDateProvider dateProvider)
        {
            _context = context;
            _salesAccessService = salesAccessService;
            _dateProvider = dateProvider;
        }

        [HttpGet]
        public IActionResult Invoices()
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return RedirectToAction("Login", "Account");

            var today = _dateProvider.Today.Date;
            var cutoff45 = today.AddDays(-45);

            // Base: pending invoices
            IQueryable<CustomerInvoiceMain> q = _context.CUSTOMER_INVOICE_MAIN
                .AsNoTracking()
                .Where(x =>
                    (x.isFinalDelivery == false || x.isFinalDelivery == null) &&
                    x.InvoiceAmt != 0 &&
                    x.Cancel == false
                );

            // Access control (reuse service)
            var repCodes = _salesAccessService.GetAccessibleRepCodes(userType, userName, salesRepCode);
            bool isAll = repCodes.Count == 1 && repCodes[0] == "__ALL__";

            if (!isAll && userType == "DIRECTOR")
            {
                var teamCodes = GetDirectorTeamCodes(userName, teamCode);
                if (!teamCodes.Any())
                    return View(new NonDeliveredInvoicesViewModel { CutoffDate = cutoff45 });

                // Director access by team code (Pat_Name)
                q = q.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
            }
            else if (!isAll)
            {
                // REP / ASM / SM access by rep codes
                if (repCodes == null || repCodes.Count == 0)
                    return View(new NonDeliveredInvoicesViewModel { CutoffDate = cutoff45 });

                q = q.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            // Join TEAM_MIS to convert Pat_Name(LocCode) -> LocShort
            var qWithTeam =
                from inv in q
                join tm in _context.TEAM_MIS.AsNoTracking()
                    on inv.Pat_Name equals tm.LocCode into gj
                from tm in gj.DefaultIfEmpty()
                select new
                {
                    inv.RefDate,
                    inv.InvoiceAmt,
                    Team = (tm != null && tm.LocShort != null && tm.LocShort != "")
                            ? tm.LocShort
                            : inv.Pat_Name // fallback if not mapped
                };

            // Under 45 days
            var under = qWithTeam
                .Where(x => x.RefDate > cutoff45)
                .GroupBy(x => x.Team)
                .Select(g => new
                {
                    Team = g.Key,
                    Cnt = g.Count(),
                    Amt = g.Sum(x => (decimal?)x.InvoiceAmt) ?? 0
                })
                .ToList();

            // Over 45 days
            var over = qWithTeam
                .Where(x => x.RefDate <= cutoff45)
                .GroupBy(x => x.Team)
                .Select(g => new
                {
                    Team = g.Key,
                    Cnt = g.Count(),
                    Amt = g.Sum(x => (decimal?)x.InvoiceAmt) ?? 0
                })
                .ToList();

            var underDict = under.ToDictionary(x => x.Team ?? "", x => (x.Cnt, x.Amt));
            var overDict = over.ToDictionary(x => x.Team ?? "", x => (x.Cnt, x.Amt));

            var allTeams = new HashSet<string>(underDict.Keys.Concat(overDict.Keys));

            var rows = allTeams
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => new NonDeliveredTeamRowVm
                {
                    Team = t,
                    Under45Count = underDict.TryGetValue(t, out var u) ? u.Cnt : 0,
                    Under45Amount = underDict.TryGetValue(t, out var u2) ? u2.Amt : 0,
                    Over45Count = overDict.TryGetValue(t, out var o) ? o.Cnt : 0,
                    Over45Amount = overDict.TryGetValue(t, out var o2) ? o2.Amt : 0
                })
                .OrderBy(x => x.Team)
                .ToList();

            var model = new NonDeliveredInvoicesViewModel
            {
                CutoffDate = cutoff45,
                Rows = rows,
                Under45CountTotal = rows.Sum(x => x.Under45Count),
                Under45AmountTotal = rows.Sum(x => x.Under45Amount),
                Over45CountTotal = rows.Sum(x => x.Over45Count),
                Over45AmountTotal = rows.Sum(x => x.Over45Amount),
            };

            ViewBag.Title = "Invoice Delivery - Values in 1LK";
            ViewBag.SubTitle = $"Cutoff: {cutoff45:yyyy-MM-dd} (<= cutoff = Over 45 days)";

            return View(model);
        }

        private List<string> GetDirectorTeamCodes(string userName, string? sessionTeamCode)
        {
            var teamCodes = _context.DIR_TEAM_MAP
                .AsNoTracking()
                .Where(x => x.UserNameDir == userName)
                .Select(x => x.TeamCode)
                .Distinct()
                .ToList();

            if (!string.IsNullOrEmpty(sessionTeamCode) && !teamCodes.Contains(sessionTeamCode))
                teamCodes.Add(sessionTeamCode);

            return teamCodes;
        }
    }
}
