using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using MIS_DEMO.Models.ViewModels;
using MIS_DEMO.Services;
using System.Diagnostics;

namespace MIS_DEMO.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IDateProvider _dateProvider;


        public HomeController(AppDbContext context, IDateProvider dateProvider)
        {
            _context = context;
            _dateProvider = dateProvider;
        }


        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");

            var model = new DashboardViewModel();

            var today = _dateProvider.Today;
            var dayStart = today.Date;
            var dayEnd = dayStart.AddDays(1);

            // -----------------------------
            // 1) TODAY SALES (your existing KPI)
            // -----------------------------
            IQueryable<SalesFact> todaySalesQuery = _context.VW_SALES_FACT
                .AsNoTracking()
                .Where(x => x.RefDate >= dayStart && x.RefDate < dayEnd && x.LineTotal > 0);

            IQueryable<VwSalesReturnFact> todayReturnQuery = _context.VW_SALES_RETURN_FACT
                .AsNoTracking()
                .Where(x => x.RefDate >= dayStart && x.RefDate < dayEnd && x.LineTotal > 0);

            // Apply access rules (same logic as your TodayTotalSales)
            if (userType == "REP")
            {
                todaySalesQuery = todaySalesQuery.Where(x => x.SalesRepCode == salesRepCode);
                todayReturnQuery = todayReturnQuery.Where(x => x.SalesRepCode == salesRepCode);
            }
            else if (userType == "DIRECTOR")
            {
                if (teamCode == "L006")
                {
                    // no filter
                }
                else
                {
                    var teamCodes = _context.DIR_TEAM_MAP
                        .AsNoTracking()
                        .Where(x => x.UserNameDir == userName)
                        .Select(x => x.TeamCode)
                        .ToList();

                    if (!string.IsNullOrEmpty(teamCode) && !teamCodes.Contains(teamCode))
                        teamCodes.Add(teamCode);

                    if (!teamCodes.Any())
                    {
                        // No teams -> no data
                        todaySalesQuery = todaySalesQuery.Where(x => false);
                        todayReturnQuery = todayReturnQuery.Where(x => false);
                    }
                    else
                    {
                        // Sales: team-based
                        todaySalesQuery = todaySalesQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));

                        // Returns: if your return view also has Pat_Name, keep this:
                        todayReturnQuery = todayReturnQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));

                        // If your returns view DOES NOT have Pat_Name for director, then you must filter returns by repCodes (like SalesController does).
                        // But based on your SalesController.PeriodDetails you ARE using Pat_Name for returns, so this is OK.
                    }
                }
            }
            else if (userType == "ASM")
            {
                var isSM = _context.WKF_MAP_SM_ASM
                    .AsNoTracking()
                    .Any(x => x.UserNameSM == userName);

                List<string> repCodes;

                if (isSM)
                {
                    var assignedASMs = _context.WKF_MAP_SM_ASM
                        .AsNoTracking()
                        .Where(x => x.UserNameSM == userName)
                        .Select(x => x.UserNameASM)
                        .ToList();

                    var asmRepCodes = _context.WKF_USER_REP_MAP
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    var repRepCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    repCodes = asmRepCodes.Union(repRepCodes).ToList();
                }
                else
                {
                    repCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .ToList();
                }

                if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode))
                    repCodes.Add(salesRepCode);

                todaySalesQuery = todaySalesQuery.Where(x => repCodes.Contains(x.SalesRepCode));
                todayReturnQuery = todayReturnQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            // Finally compute both
            model.TodayTotalSales = todaySalesQuery.Sum(x => (decimal?)x.LineTotal) ?? 0;
            model.TodayTotalReturns = todayReturnQuery.Sum(x => (decimal?)x.LineTotal) ?? 0;



            // -----------------------------
            // 2) NON-DELIVERED KPI (U.45 and O.45)
            // -----------------------------
            var cutoff45 = dayStart.AddDays(-45); // 45 days before "today"

            var pendingQuery = _context.CUSTOMER_INVOICE_MAIN
                .AsNoTracking()
                .Where(x =>
                    (x.isFinalDelivery == false || x.isFinalDelivery == null) &&
                    x.InvoiceAmt != 0 &&
                    x.Cancel == false
                );

            // Apply same access rules:
            if (userType == "REP")
            {
                pendingQuery = pendingQuery.Where(x => x.SalesRepCode == salesRepCode);
            }
            else if (userType == "ASM")
            {
                // For ASM/SM, reuse the repCodes logic (same as sales KPI)
                // We'll rebuild repCodes quickly here (no extra services needed)

                var isSM = _context.WKF_MAP_SM_ASM
                    .AsNoTracking()
                    .Any(x => x.UserNameSM == userName);

                List<string> repCodes;

                if (isSM)
                {
                    var assignedASMs = _context.WKF_MAP_SM_ASM
                        .AsNoTracking()
                        .Where(x => x.UserNameSM == userName)
                        .Select(x => x.UserNameASM)
                        .ToList();

                    var asmRepCodes = _context.WKF_USER_REP_MAP
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    var repRepCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    repCodes = asmRepCodes.Union(repRepCodes).ToList();
                }
                else
                {
                    repCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .ToList();
                }

                if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode))
                    repCodes.Add(salesRepCode);

                pendingQuery = pendingQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }
            else if (userType == "DIRECTOR")
            {
                if (teamCode == "L006")
                {
                    // NO FILTER (all pending invoices)
                }
                else
                {
                    var teamCodes = _context.DIR_TEAM_MAP
                        .AsNoTracking()
                        .Where(x => x.UserNameDir == userName)
                        .Select(x => x.TeamCode)
                        .ToList();

                    if (!string.IsNullOrEmpty(teamCode) && !teamCodes.Contains(teamCode))
                        teamCodes.Add(teamCode);

                    // CUSTOMER_INVOICE_MAIN has Pat_Name, so filter by that
                    if (teamCodes.Any())
                    {
                        pendingQuery = pendingQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
                    }
                    else
                    {
                        // No mapped teams => no data
                        pendingQuery = pendingQuery.Where(x => false);
                    }
                }
            }

            // Now compute amounts
            model.NonDeliveredUnder45 = pendingQuery
                .Where(x => x.RefDate > cutoff45)   // within last 45 days
                .Sum(x => (decimal?)x.InvoiceAmt) ?? 0;

            model.NonDeliveredOver45 = pendingQuery
                .Where(x => x.RefDate <= cutoff45)  // older than 45 days
                .Sum(x => (decimal?)x.InvoiceAmt) ?? 0;


            // -----------------------------
            // 3) TEAM WISE NET SALE KPI (This Month vs Last Month)
            // -----------------------------
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonthStart = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            // base queries (gross sales + returns)
            IQueryable<SalesFact> teamSalesQuery = _context.VW_SALES_FACT
                .AsNoTracking()
                .Where(x => x.LineTotal > 0);

            IQueryable<VwSalesReturnFact> teamReturnQuery = _context.VW_SALES_RETURN_FACT
                .AsNoTracking()
                .Where(x => x.LineTotal > 0);

            // apply SAME access rules
            if (userType == "REP")
            {
                teamSalesQuery = teamSalesQuery.Where(x => x.SalesRepCode == salesRepCode);
                teamReturnQuery = teamReturnQuery.Where(x => x.SalesRepCode == salesRepCode);
            }
            else if (userType == "DIRECTOR")
            {
                if (teamCode == "L006")
                {
                    // no filter
                }
                else
                {
                    var teamCodes = _context.DIR_TEAM_MAP
                        .AsNoTracking()
                        .Where(x => x.UserNameDir == userName)
                        .Select(x => x.TeamCode)
                        .ToList();

                    if (!string.IsNullOrEmpty(teamCode) && !teamCodes.Contains(teamCode))
                        teamCodes.Add(teamCode);

                    if (teamCodes.Any())
                    {
                        // Director: team-based
                        teamSalesQuery = teamSalesQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
                        teamReturnQuery = teamReturnQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
                    }
                    else
                    {
                        teamSalesQuery = teamSalesQuery.Where(x => false);
                        teamReturnQuery = teamReturnQuery.Where(x => false);
                    }
                }
            }
            else if (userType == "ASM")
            {
                var isSM = _context.WKF_MAP_SM_ASM
                    .AsNoTracking()
                    .Any(x => x.UserNameSM == userName);

                List<string> repCodes;

                if (isSM)
                {
                    var assignedASMs = _context.WKF_MAP_SM_ASM
                        .AsNoTracking()
                        .Where(x => x.UserNameSM == userName)
                        .Select(x => x.UserNameASM)
                        .ToList();

                    var asmRepCodes = _context.WKF_USER_REP_MAP
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    var repRepCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => assignedASMs.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();

                    repCodes = asmRepCodes.Union(repRepCodes).ToList();
                }
                else
                {
                    repCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .ToList();
                }

                if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode))
                    repCodes.Add(salesRepCode);

                teamSalesQuery = teamSalesQuery.Where(x => repCodes.Contains(x.SalesRepCode));
                teamReturnQuery = teamReturnQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            // ---- THIS MONTH totals by team (LocShort)
            var thisMonthSales = teamSalesQuery
                .Where(x => x.RefDate >= thisMonthStart && x.RefDate < nextMonthStart)
                .Where(x => !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort)
                .Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 })
                .ToList();

            var thisMonthReturns = teamReturnQuery
                .Where(x => x.RefDate >= thisMonthStart && x.RefDate < nextMonthStart)
                .Where(x => !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort)
                .Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 })
                .ToList();

            // ---- LAST MONTH totals by team (LocShort)
            var lastMonthSales = teamSalesQuery
                .Where(x => x.RefDate >= lastMonthStart && x.RefDate < thisMonthStart)
                .Where(x => !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort)
                .Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 })
                .ToList();

            var lastMonthReturns = teamReturnQuery
                .Where(x => x.RefDate >= lastMonthStart && x.RefDate < thisMonthStart)
                .Where(x => !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort)
                .Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 })
                .ToList();

            // Convert to dictionaries
            var thisSalesDict = thisMonthSales.ToDictionary(x => x.LocShort, x => x.Total);
            var thisRetDict = thisMonthReturns.ToDictionary(x => x.LocShort, x => x.Total);

            var lastSalesDict = lastMonthSales.ToDictionary(x => x.LocShort, x => x.Total);
            var lastRetDict = lastMonthReturns.ToDictionary(x => x.LocShort, x => x.Total);

            // Union of teams appearing in any list
            var allTeams = new HashSet<string>(
                thisSalesDict.Keys
                    .Concat(thisRetDict.Keys)
                    .Concat(lastSalesDict.Keys)
                    .Concat(lastRetDict.Keys)
            );

            // Build NET rows
            model.TeamWiseSales = allTeams
                .Select(team => new TeamWiseSaleRowVm
                {
                    LocShort = team,
                    ThisMonth = (thisSalesDict.TryGetValue(team, out var ts) ? ts : 0)
                                - (thisRetDict.TryGetValue(team, out var tr) ? tr : 0),
                    LastMonth = (lastSalesDict.TryGetValue(team, out var ls) ? ls : 0)
                                - (lastRetDict.TryGetValue(team, out var lr) ? lr : 0)
                })
                .OrderByDescending(x => x.ThisMonth)
                .ToList();

            // Totals (NET totals)
            model.TeamWiseThisMonthTotal = model.TeamWiseSales.Sum(x => x.ThisMonth);
            model.TeamWiseLastMonthTotal = model.TeamWiseSales.Sum(x => x.LastMonth);


            return View(model);
        }


    }
}
