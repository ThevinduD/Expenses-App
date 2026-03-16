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

        // ==========================================
        // MAIN DASHBOARD LOAD
        // ==========================================
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

            // 1) TODAY SALES & RETURNS
            var todaySalesQuery = _context.VW_SALES_FACT.AsNoTracking().Where(x => x.RefDate >= dayStart && x.RefDate < dayEnd && x.LineTotal > 0);
            var todayReturnQuery = _context.VW_SALES_RETURN_FACT.AsNoTracking().Where(x => x.RefDate >= dayStart && x.RefDate < dayEnd && x.LineTotal > 0);

            todaySalesQuery = ApplySalesRoleFilter(todaySalesQuery, userName, userType, salesRepCode, teamCode);
            todayReturnQuery = ApplySalesRoleFilter(todayReturnQuery, userName, userType, salesRepCode, teamCode);

            model.TodayTotalSales = todaySalesQuery.Sum(x => (decimal?)x.LineTotal) ?? 0;
            model.TodayTotalReturns = todayReturnQuery.Sum(x => (decimal?)x.LineTotal) ?? 0;


            // 2) NON-DELIVERED KPI
            var cutoff45 = dayStart.AddDays(-45);
            var pendingQuery = _context.CUSTOMER_INVOICE_MAIN.AsNoTracking()
                .Where(x => (x.isFinalDelivery == false || x.isFinalDelivery == null) && x.InvoiceAmt != 0 && x.Cancel == false);

            pendingQuery = ApplyInvoiceRoleFilter(pendingQuery, userName, userType, salesRepCode, teamCode);

            model.NonDeliveredUnder45 = pendingQuery.Where(x => x.RefDate > cutoff45).Sum(x => (decimal?)x.InvoiceAmt) ?? 0;
            model.NonDeliveredOver45 = pendingQuery.Where(x => x.RefDate <= cutoff45).Sum(x => (decimal?)x.InvoiceAmt) ?? 0;


            // 3) DELIVERED INVOICES
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonthStart = thisMonthStart.AddMonths(1);

            var deliveredQ = _context.CUSTOMER_INVOICE_MAIN.AsNoTracking()
                .Where(x => x.isFinalDelivery == true && x.Cancel == false && x.InvoiceAmt != 0 && x.FDeliveryDate >= thisMonthStart && x.FDeliveryDate < nextMonthStart);

            deliveredQ = ApplyInvoiceRoleFilter(deliveredQ, userName, userType, salesRepCode, teamCode);
            model.ThisMonthDeliveredTotal = deliveredQ.Sum(x => (decimal?)x.InvoiceAmt) ?? 0;


            // 4) STOCK KPI
            var stockQuery = _context.VW_STOCK_TEAM_VALUE.AsNoTracking().Where(x => x.StockQty > 0);
            stockQuery = ApplyStockRoleFilter(stockQuery, userName, userType, salesRepCode, teamCode);

            var teamStock = stockQuery
                .GroupBy(x => new { x.TeamCode, x.TeamName })
                .Select(g => new TeamStockRowVm
                {
                    TeamName = g.Key.TeamName ?? "UNASSIGNED",
                    StockValue = g.Sum(x => (decimal?)x.StockValue) ?? 0,
                    ExpiringSoonValue = g.Where(x => x.ExpiryDays >= 0 && x.ExpiryDays <= 60).Sum(x => (decimal?)x.StockValue) ?? 0
                })
                .OrderByDescending(x => x.StockValue)
                .ToList();

            model.TeamStockValues = teamStock;
            model.TotalStockValue = teamStock.Sum(x => x.StockValue);
            model.TotalExpiringSoonValue = teamStock.Sum(x => x.ExpiringSoonValue);


            return View(model);
        }

        // ==========================================
        // AJAX KPI LOADER
        // ==========================================
        [HttpGet]
        public IActionResult LoadTeamWiseSalesKpi(int monthOffset = 0)
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            var referenceDate = _dateProvider.Today.AddMonths(monthOffset);
            var thisMonthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var nextMonthStart = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var teamSalesQuery = _context.VW_SALES_FACT.AsNoTracking().Where(x => x.LineTotal > 0);
            var teamReturnQuery = _context.VW_SALES_RETURN_FACT.AsNoTracking().Where(x => x.LineTotal > 0);

            // Using the new helper method!
            teamSalesQuery = ApplySalesRoleFilter(teamSalesQuery, userName, userType, salesRepCode, teamCode);
            teamReturnQuery = ApplySalesRoleFilter(teamReturnQuery, userName, userType, salesRepCode, teamCode);

            var thisMonthSales = teamSalesQuery.Where(x => x.RefDate >= thisMonthStart && x.RefDate < nextMonthStart && !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort).Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 }).ToList();
            var thisMonthReturns = teamReturnQuery.Where(x => x.RefDate >= thisMonthStart && x.RefDate < nextMonthStart && !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort).Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 }).ToList();
            var lastMonthSales = teamSalesQuery.Where(x => x.RefDate >= lastMonthStart && x.RefDate < thisMonthStart && !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort).Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 }).ToList();
            var lastMonthReturns = teamReturnQuery.Where(x => x.RefDate >= lastMonthStart && x.RefDate < thisMonthStart && !string.IsNullOrEmpty(x.LocShort))
                .GroupBy(x => x.LocShort).Select(g => new { LocShort = g.Key, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 }).ToList();

            var thisSalesDict = thisMonthSales.ToDictionary(x => x.LocShort, x => x.Total);
            var thisRetDict = thisMonthReturns.ToDictionary(x => x.LocShort, x => x.Total);
            var lastSalesDict = lastMonthSales.ToDictionary(x => x.LocShort, x => x.Total);
            var lastRetDict = lastMonthReturns.ToDictionary(x => x.LocShort, x => x.Total);

            var allTeams = new HashSet<string>(thisSalesDict.Keys.Concat(thisRetDict.Keys).Concat(lastSalesDict.Keys).Concat(lastRetDict.Keys));

            var teamWiseSalesList = allTeams
                .Select(team => new TeamWiseSaleRowVm
                {
                    LocShort = team,
                    ThisMonth = (thisSalesDict.GetValueOrDefault(team, 0)) - (thisRetDict.GetValueOrDefault(team, 0)),
                    LastMonth = (lastSalesDict.GetValueOrDefault(team, 0)) - (lastRetDict.GetValueOrDefault(team, 0))
                })
                .OrderByDescending(x => x.ThisMonth)
                .ToList();

            var vm = new TeamWiseSalesKpiViewModel
            {
                TeamWiseSales = teamWiseSalesList,
                TeamWiseThisMonthTotal = teamWiseSalesList.Sum(x => x.ThisMonth),
                TeamWiseLastMonthTotal = teamWiseSalesList.Sum(x => x.LastMonth),
                CurrentMonthOffset = monthOffset,
                ThisMonthLabel = thisMonthStart.ToString("MMM yyyy"),
                LastMonthLabel = lastMonthStart.ToString("MMM yyyy")
            };

            return PartialView("_TeamWiseSalesKPI", vm);
        }

        [HttpGet]
        public IActionResult LoadTopPerformersKpi(int monthOffset = 0, bool isTop = true)
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            // 1. Shift the date using the monthOffset
            var referenceDate = _dateProvider.Today.AddMonths(monthOffset);
            var currentMonthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1);

            var topSalesQ = _context.VW_SALES_FACT.AsNoTracking()
                .Where(x => x.RefDate >= currentMonthStart && x.RefDate < currentMonthEnd && x.LineTotal > 0);

            var topReturnQ = _context.VW_SALES_RETURN_FACT.AsNoTracking()
                .Where(x => x.RefDate >= currentMonthStart && x.RefDate < currentMonthEnd && x.LineTotal > 0);

            // 2. Apply Security Filters
            topSalesQ = ApplySalesRoleFilter(topSalesQ, userName, userType, salesRepCode, teamCode);
            topReturnQ = ApplySalesRoleFilter(topReturnQ, userName, userType, salesRepCode, teamCode);

            // 3. Aggregate
            var repSales = topSalesQ
                .Where(x => !string.IsNullOrEmpty(x.SalesRepName))
                .GroupBy(x => new { x.LocShort, x.SalesRepName })
                .Select(g => new { Team = g.Key.LocShort, Rep = g.Key.SalesRepName, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 })
                .ToList();

            var repReturns = topReturnQ
                .Where(x => !string.IsNullOrEmpty(x.SalesRepName))
                .GroupBy(x => new { x.LocShort, x.SalesRepName })
                .Select(g => new { Team = g.Key.LocShort, Rep = g.Key.SalesRepName, Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0 })
                .ToList();

            var repSalesDict = repSales.ToDictionary(x => (Team: x.Team ?? "", Rep: x.Rep ?? ""), x => x.Total);
            var repRetDict = repReturns.ToDictionary(x => (Team: x.Team ?? "", Rep: x.Rep ?? ""), x => x.Total);

            var allRepKeys = new HashSet<(string Team, string Rep)>(
                repSalesDict.Keys.Concat(repRetDict.Keys)
            );

            // Calculate all Net Sales unsorted
            var allRepsUnsorted = allRepKeys.Select(k => new TopRepSaleRow
            {
                Team = string.IsNullOrEmpty(k.Team) ? "UNASSIGNED" : k.Team,
                RepName = k.Rep,
                NetSale = repSalesDict.GetValueOrDefault(k, 0) - repRetDict.GetValueOrDefault(k, 0)
            });

            // 4. Sort based on the toggle (isTop)
            var topRepsList = isTop
                ? allRepsUnsorted.OrderByDescending(x => x.NetSale).Take(10).ToList() // Best performers
                : allRepsUnsorted.OrderBy(x => x.NetSale).Take(10).ToList();          // Lowest performers

            // 5. Build the VM for the Partial View
            var vm = new TopPerformersKpiViewModel
            {
                TopReps = topRepsList,
                CurrentMonthOffset = monthOffset,
                MonthLabel = currentMonthStart.ToString("MMM yyyy"), // Sets text like "Mar 2026"
                IsTop = isTop // Pass the toggle state to the view
            };

            return PartialView("_TopPerformersKPI", vm);
        }

        // ==========================================
        // PRIVATE HELPER METHODS (The Clean Up)
        // ==========================================

        private IQueryable<T> ApplySalesRoleFilter<T>(IQueryable<T> query, string userName, string userType, string salesRepCode, string teamCode) where T : class
        {
            // We use dynamic expression trees here to handle both VW_SALES_FACT and VW_SALES_RETURN_FACT cleanly
            if (userType == "REP")
            {
                return query.Where(e => EF.Property<string>(e, "SalesRepCode") == salesRepCode);
            }
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
            return query; // Admin / L006 sees all
        }

        private IQueryable<CustomerInvoiceMain> ApplyInvoiceRoleFilter(IQueryable<CustomerInvoiceMain> query, string userName, string userType, string salesRepCode, string teamCode)
        {
            if (userType == "REP")
            {
                return query.Where(x => x.SalesRepCode == salesRepCode);
            }
            if (userType == "DIRECTOR" && teamCode != "L006")
            {
                var teamCodes = _context.DIR_TEAM_MAP.Where(x => x.UserNameDir == userName).Select(x => x.TeamCode).ToList();
                if (!string.IsNullOrEmpty(teamCode) && !teamCodes.Contains(teamCode)) teamCodes.Add(teamCode);

                if (teamCodes.Any()) return query.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
                return query.Where(x => false);
            }
            if (userType == "ASM" || userType == "OTHER")
            {
                var repCodes = GetAllowedRepCodes(userName);
                if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode)) repCodes.Add(salesRepCode);
                return query.Where(x => repCodes.Contains(x.SalesRepCode));
            }
            return query;
        }

        private IQueryable<StockTeamValue> ApplyStockRoleFilter(IQueryable<StockTeamValue> query, string userName, string userType, string salesRepCode, string teamCode)
        {
            if (userType == "REP")
            {
                var allowedSupCodes = from ra in _context.WKF_MAP_REP_ASM
                                      join sa in _context.SUPPLIER_ASM on ra.UserName equals sa.ASMCODE
                                      where ra.SalesRepCode == salesRepCode
                                      select sa.SUPCODE;
                return query.Where(x => allowedSupCodes.Contains(x.SupCode) && x.TeamCode != "L002");
            }
            if (userType == "ASM" || userType == "SM" || userType == "OTHER")
            {
                var supCodes = _context.SUPPLIER_ASM.Where(x => x.ASMCODE == userName).Select(x => x.SUPCODE);
                return query.Where(x => supCodes.Contains(x.SupCode) && x.TeamCode != "L002");
            }
            if (userType == "DIRECTOR" && teamCode != "L006")
            {
                var teamCodes = _context.DIR_TEAM_MAP.Where(x => x.UserNameDir == userName).Select(x => x.TeamCode);
                return query.Where(x => teamCodes.Contains(x.TeamCode));
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