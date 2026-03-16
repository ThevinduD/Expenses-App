using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using MIS_DEMO.Models.ViewModels;
using MIS_DEMO.Services;

namespace MIS_DEMO.Controllers
{
    public class PerformanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDateProvider _dateProvider;

        public PerformanceController(AppDbContext context, IDateProvider dateProvider)
        {
            _context = context;
            _dateProvider = dateProvider;
        }

        [HttpGet]
        public IActionResult RepPerformanceDetails(int monthOffset = 0)
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return RedirectToAction("Login", "Account");

            // 1. Date Calculations
            var referenceDate = _dateProvider.Today.AddMonths(monthOffset);
            var monthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            int targetMonthNo = referenceDate.Month;

            // 2. Fetch Gross Sales & Returns
            var salesQ = _context.VW_SALES_FACT.AsNoTracking()
                .Where(x => x.RefDate >= monthStart && x.RefDate < monthEnd && x.LineTotal > 0);

            var retQ = _context.VW_SALES_RETURN_FACT.AsNoTracking()
                .Where(x => x.RefDate >= monthStart && x.RefDate < monthEnd && x.LineTotal > 0);

            // 3. Apply the Security Matrix
            salesQ = ApplySalesRoleFilter(salesQ, userName, userType, salesRepCode, teamCode);
            retQ = ApplySalesRoleFilter(retQ, userName, userType, salesRepCode, teamCode);

            // 4. Fetch Targets for the requested month
            var targetsList = _context.TARGET_MONTHS_REP_SPECIAL.AsNoTracking()
                .Where(x => x.MonthNo == targetMonthNo && x.SalesRepCode != null)
                .ToList();

            var targetDict = targetsList
                .GroupBy(x => x.SalesRepCode ?? "") // ?? "" prevents Null-Key crashes
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TargetActual ?? 0m));

            // 5. Aggregate Sales & Returns by Rep
            var salesAgg = salesQ
                .Where(x => !string.IsNullOrEmpty(x.SalesRepCode))
                .GroupBy(x => new { x.LocShort, x.SalesRepCode, x.SalesRepName })
                .Select(g => new {
                    Team = g.Key.LocShort,
                    RepCode = g.Key.SalesRepCode,
                    RepName = g.Key.SalesRepName,
                    Gross = g.Sum(x => (decimal?)x.LineTotal) ?? 0m
                }).ToList();

            var retAgg = retQ
                .Where(x => !string.IsNullOrEmpty(x.SalesRepCode))
                .GroupBy(x => new { x.LocShort, x.SalesRepCode, x.SalesRepName })
                .Select(g => new {
                    Team = g.Key.LocShort,
                    RepCode = g.Key.SalesRepCode,
                    RepName = g.Key.SalesRepName,
                    Ret = g.Sum(x => (decimal?)x.LineTotal) ?? 0m
                }).ToList();


            // 6. Combine Everything Safely into Dictionaries (USING COMPOSITE KEYS)
            var salesDict = salesAgg
                .GroupBy(x => (Team: x.Team ?? "", RepCode: x.RepCode ?? ""))
                .ToDictionary(g => g.Key, g => g.First());

            var retDict = retAgg
                .GroupBy(x => (Team: x.Team ?? "", RepCode: x.RepCode ?? ""))
                .ToDictionary(g => g.Key, g => g.First());

            // Get every unique Team + Rep Code combo
            var allRepKeys = new HashSet<(string Team, string RepCode)>(
                salesDict.Keys.Concat(retDict.Keys)
            );

            var rows = allRepKeys.Select(k => {
                salesDict.TryGetValue(k, out var s);
                retDict.TryGetValue(k, out var r);

                return new RepPerformanceRow
                {
                    Team = string.IsNullOrEmpty(k.Team) ? "UNASSIGNED" : k.Team,
                    RepCode = k.RepCode,
                    RepName = s?.RepName ?? r?.RepName ?? "Unknown",
                    GrossSale = s?.Gross ?? 0m,
                    ReturnSale = r?.Ret ?? 0m,
                    NetSale = (s?.Gross ?? 0m) - (r?.Ret ?? 0m),

                    // Notice how Target still looks up ONLY the RepCode from the target table!
                    Target = targetDict.GetValueOrDefault(k.RepCode, 0m)
                };
            })
            .OrderByDescending(x => x.NetSale) // Strictly sorts highest net sale to lowest
            .ToList();

            // 7. Build the final ViewModel
            var vm = new RepPerformanceDetailsViewModel
            {
                MonthLabel = monthStart.ToString("MMMM yyyy"),
                MonthOffset = monthOffset,
                Rows = rows,
                TotalGross = rows.Sum(x => x.GrossSale),
                TotalReturn = rows.Sum(x => x.ReturnSale),
                TotalNet = rows.Sum(x => x.NetSale),
                TotalTarget = targetDict.Values.Sum() // Changed to accurately sum unique targets
            };

            return View(vm);
        }

        // ==========================================
        // PRIVATE SECURITY FILTERS
        // ==========================================
        private IQueryable<T> ApplySalesRoleFilter<T>(IQueryable<T> query, string userName, string userType, string salesRepCode, string teamCode) where T : class
        {
            if (userType == "REP")
                return query.Where(e => EF.Property<string>(e, "SalesRepCode") == salesRepCode);

            if (userType == "DIRECTOR" && teamCode != "L006")
            {
                var teamCodes = _context.DIR_TEAM_MAP.Where(x => x.UserNameDir == userName).Select(x => x.TeamCode).ToList();
                if (!string.IsNullOrEmpty(teamCode) && !teamCodes.Contains(teamCode)) teamCodes.Add(teamCode);

                if (teamCodes.Any()) return query.Where(e => EF.Property<string>(e, "Pat_Name") != null && teamCodes.Contains(EF.Property<string>(e, "Pat_Name")));
                return query.Where(e => false);
            }
            if (userType == "ASM" || userType == "OTHER")
            {
                var repCodes = GetAllowedRepCodes(userName);
                if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode)) repCodes.Add(salesRepCode);
                return query.Where(e => repCodes.Contains(EF.Property<string>(e, "SalesRepCode")));
            }
            return query;
        }

        private List<string> GetAllowedRepCodes(string userName)
        {
            var isSM = _context.WKF_MAP_SM_ASM.Any(x => x.UserNameSM == userName);
            if (isSM)
            {
                var assignedASMs = _context.WKF_MAP_SM_ASM.Where(x => x.UserNameSM == userName).Select(x => x.UserNameASM).ToList();
                var asmRepCodes = _context.WKF_USER_REP_MAP.Where(x => assignedASMs.Contains(x.UserName)).Select(x => x.SalesRepCode).ToList();
                var repRepCodes = _context.WKF_MAP_REP_ASM.Where(x => assignedASMs.Contains(x.UserName)).Select(x => x.SalesRepCode).ToList();
                return asmRepCodes.Union(repRepCodes).ToList();
            }
            return _context.WKF_MAP_REP_ASM.Where(x => x.UserName == userName).Select(x => x.SalesRepCode).ToList();
        }
    }
}