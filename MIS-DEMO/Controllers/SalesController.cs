using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using MIS_DEMO.Models.ViewModels;
using MIS_DEMO.Services;
using System.Globalization;


namespace MIS_DEMO.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SalesAccessService _salesAccessService;
        private readonly IMemoryCache _cache;
        private readonly IDateProvider _dateProvider;


        public SalesController(
            AppDbContext context,
            SalesAccessService salesAccessService,
            IMemoryCache cache, IDateProvider dateProvider)
        {
            _context = context;
            _salesAccessService = salesAccessService;
            _cache = cache;
            _dateProvider = dateProvider;
        }

        public IActionResult Details()
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode"); // ✅ add this

            ViewBag.TodayIso = _dateProvider.Today.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return RedirectToAction("Login", "Account");

            var today = _dateProvider.Today;
            var yesterday = today.AddDays(-1);
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonthStart = thisMonthStart.AddMonths(1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);

            var cacheKey = $"salesSummary:{userName}:{userType}:{salesRepCode}:{today:yyyyMMdd}";
            if (_cache.TryGetValue(cacheKey, out SalesSummaryViewModel cachedModel))
                return View(cachedModel);

            var repCodes = _salesAccessService.GetAccessibleRepCodes(userType, userName, salesRepCode);
            bool isAll = repCodes.Count == 1 && repCodes[0] == "__ALL__";

            var salesQuery = _context.VW_SALES_FACT.AsNoTracking().Where(x => x.LineTotal > 0);
            var returnQuery = _context.VW_SALES_RETURN_FACT.AsNoTracking().Where(x => x.LineTotal > 0);

            // ✅ SALES FILTERING
            if (isAll)
            {
                // L006 director => no filter
            }
            else if (userType == "DIRECTOR")
            {
                var teamCodes = GetDirectorTeamCodes(userName, teamCode);

                if (!teamCodes.Any())
                    return View(new SalesSummaryViewModel());

                // IMPORTANT: director sales must be by TEAM
                salesQuery = salesQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));

                // returns: keep rep-based (no team field in return view)
                if (!repCodes.Any())
                    return View(new SalesSummaryViewModel());

                returnQuery = returnQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }
            else
            {
                if (!repCodes.Any())
                    return View(new SalesSummaryViewModel());

                salesQuery = salesQuery.Where(x => repCodes.Contains(x.SalesRepCode));
                returnQuery = returnQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            var model = new SalesSummaryViewModel
            {
                TodaySales = salesQuery.Where(x => x.RefDate >= today && x.RefDate < today.AddDays(1))
                                       .Sum(x => (decimal?)x.LineTotal) ?? 0,

                YesterdaySales = salesQuery.Where(x => x.RefDate >= yesterday && x.RefDate < today)
                                           .Sum(x => (decimal?)x.LineTotal) ?? 0,

                ThisMonthSales = salesQuery.Where(x => x.RefDate >= thisMonthStart && x.RefDate < nextMonthStart)
                                           .Sum(x => (decimal?)x.LineTotal) ?? 0,

                LastMonthSales = salesQuery.Where(x => x.RefDate >= lastMonthStart && x.RefDate < thisMonthStart)
                                           .Sum(x => (decimal?)x.LineTotal) ?? 0,

                TodayReturns = returnQuery.Where(x => x.RefDate >= today && x.RefDate < today.AddDays(1))
                                          .Sum(x => (decimal?)x.LineTotal) ?? 0,

                YesterdayReturns = returnQuery.Where(x => x.RefDate >= yesterday && x.RefDate < today)
                                              .Sum(x => (decimal?)x.LineTotal) ?? 0,

                ThisMonthReturns = returnQuery.Where(x => x.RefDate >= thisMonthStart && x.RefDate < nextMonthStart)
                                              .Sum(x => (decimal?)x.LineTotal) ?? 0,

                LastMonthReturns = returnQuery.Where(x => x.RefDate >= lastMonthStart && x.RefDate < thisMonthStart)
                                              .Sum(x => (decimal?)x.LineTotal) ?? 0
            };

            _cache.Set(cacheKey, model, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });

            return View(model);
        }

        public IActionResult PeriodDetails(string period, string? from, string? to)
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");


            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return RedirectToAction("Login", "Account");

            var repCodes = _salesAccessService.GetAccessibleRepCodes(userType, userName, salesRepCode);

            bool isAll = repCodes.Count == 1 && repCodes[0] == "__ALL__";
            if (!isAll && !repCodes.Any())
                return View("TodayDetails", new TodaySalesDetailsViewModel());

            // Dev date (replace with DateTime.Today later)
            var baseToday = _dateProvider.Today;

            DateTime startDate;
            DateTime endDateExclusive;
            string title;

            var p = (period ?? "").ToLower();

            if (p == "select")
            {
                // If user hasn't selected dates yet, show a default range (example: last 7 days)
                if (!DateTime.TryParse(from, out var fromDt) || !DateTime.TryParse(to, out var toDt))
                {
                    startDate = baseToday.AddDays(-6).Date;
                    endDateExclusive = baseToday.AddDays(1).Date; // inclusive UX
                    title = "Custom Range Details";
                }
                else
                {
                    // inclusive end-date UX: to=2025-11-20 means include the full day
                    startDate = fromDt.Date;
                    endDateExclusive = toDt.Date.AddDays(1);
                    title = "Custom Range Details";
                }
            }
            else
            {
                switch (p)
                {
                    case "yesterday":
                        startDate = baseToday.AddDays(-1);
                        endDateExclusive = baseToday;
                        title = "Yesterday Details";
                        break;

                    case "thismonth":
                        startDate = new DateTime(baseToday.Year, baseToday.Month, 1);
                        endDateExclusive = startDate.AddMonths(1);
                        title = "This Month Details";
                        break;

                    case "lastmonth":
                        var thisMonthStart = new DateTime(baseToday.Year, baseToday.Month, 1);
                        startDate = thisMonthStart.AddMonths(-1);
                        endDateExclusive = thisMonthStart;
                        title = "Last Month Details";
                        break;

                    case "today":
                    default:
                        startDate = baseToday;
                        endDateExclusive = baseToday.AddDays(1);
                        title = "Today Details";
                        break;
                }
            }

            var model = new TodaySalesDetailsViewModel
            {
                Date = startDate,
                FromDate = startDate,
                ToDate = endDateExclusive.AddDays(-1) // for UI display
            };

            // SALES
            IQueryable<SalesFact> salesQuery = _context.VW_SALES_FACT.AsNoTracking()
                    .Where(x => x.RefDate >= startDate && x.RefDate < endDateExclusive && x.LineTotal > 0);

            if (isAll)
            {
                // no filter
            }
            else if (userType == "DIRECTOR")
            {
                var teamCodes = GetDirectorTeamCodes(userName, teamCode);
                salesQuery = salesQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
            }
            else
            {
                salesQuery = salesQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            const int MAX_ROWS = 1000;

            model.SalesTotal = salesQuery.Sum(x => (decimal?)x.LineTotal) ?? 0;
            model.SalesTotalRows = salesQuery.Count();
            model.MaxRows = MAX_ROWS;
            model.SalesIsTruncated = model.SalesTotalRows > MAX_ROWS;

            model.SalesLines = salesQuery
                .Select(x => new SalesLineViewModel
                {
                    RefDate = x.RefDate,
                    CusCode = x.CusCode,
                    CusName = x.CusName,
                    InvoDocNo = x.InvoDocNo,
                    ItemCode = x.ItemCode,
                    Description = x.ItemDescription,
                    Qty = x.Qty,
                    SoldPrice = x.SoldPrice,
                    LineTotal = x.LineTotal,
                    SupName = x.SupName,
                    SalesRepCode = x.SalesRepCode,
                    SalesRepName = x.SalesRepName,
                    LocShort = x.LocShort
                })
                .OrderByDescending(x => x.LineTotal)
                .Take(MAX_ROWS)
                .ToList();

            // RETURNS
            IQueryable<VwSalesReturnFact> returnQuery = _context.VW_SALES_RETURN_FACT
                .AsNoTracking()
                .Where(x => x.RefDate >= startDate && x.RefDate < endDateExclusive && x.LineTotal > 0);

            if (isAll)
            {
                // no filter
            }
            else if (userType == "DIRECTOR")
            {
                var teamCodes = GetDirectorTeamCodes(userName, teamCode);
                returnQuery = returnQuery.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
            }
            else
            {
                returnQuery = returnQuery.Where(x => repCodes.Contains(x.SalesRepCode));
            }


            model.ReturnTotal = returnQuery.Sum(x => (decimal?)x.LineTotal) ?? 0;
            model.ReturnTotalRows = returnQuery.Count();
            model.ReturnIsTruncated = model.ReturnTotalRows > MAX_ROWS;

            model.ReturnLines = returnQuery
                .Select(x => new ReturnLineViewModel
                {
                    RefDate = x.RefDate,
                    CusCode = x.CusCode,
                    CusName = x.CusName,
                    RtnDocNo = x.RtnDocNo,
                    InvoDocNo = x.InvoDocNo,
                    ItemCode = x.ItemCode,
                    Description = x.Description,
                    Qty = x.Qty,
                    ReturnedPrice = x.ReturnedPrice,
                    LineTotal = x.LineTotal,
                    SupName = x.SupName,
                    SalesRepCode = x.SalesRepCode,
                    SalesRepName = x.SalesRepName,
                    LocShort = x.LocShort
                })
                .OrderByDescending(x => x.LineTotal)
                .Take(MAX_ROWS)
                .ToList();

            ViewBag.PeriodTitle = title;
            ViewBag.PeriodRange = $"{startDate:yyyy-MM-dd} → {endDateExclusive.AddDays(-1):yyyy-MM-dd}";
            ViewBag.Period = p; // so cshtml can show range form only for select

            return View("TodayDetails", model);
        }

        [HttpGet]
        public IActionResult ProductWiseData(string? period, string? from, string? to, string metric = "value")
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");

            ViewBag.TodayIso = _dateProvider.Today.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            var cacheKey = $"pie:{userName}:{userType}:{salesRepCode}:{period}:{from}:{to}:{metric}";

            if (_cache.TryGetValue(cacheKey, out PieChartDataVm cached))
            {
                return Json(cached);
            }

            var repCodes = _salesAccessService.GetAccessibleRepCodes(userType, userName, salesRepCode);

            bool isAll = repCodes.Count == 1 && repCodes[0] == "__ALL__";

            if (!isAll && !repCodes.Any())
                return Json(new PieChartDataVm());

            // DEV base date (change to DateTime.Today later)
            var baseToday = _dateProvider.Today;

            DateTime startDate;
            DateTime endDateExclusive;

            // If "from/to" provided => use range mode
            bool hasFrom = DateTime.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fromDate);

            bool hasTo = DateTime.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var toDate);

            if (hasFrom && hasTo)
            {
                startDate = fromDate.Date;
                endDateExclusive = toDate.Date.AddDays(1); // inclusive end-date UX
            }
            else
            {
                // Otherwise use "period"
                switch ((period ?? "today").ToLower())
                {
                    case "yesterday":
                        startDate = baseToday.AddDays(-1);
                        endDateExclusive = baseToday;
                        break;

                    case "thismonth":
                        startDate = new DateTime(baseToday.Year, baseToday.Month, 1);
                        endDateExclusive = startDate.AddMonths(1);
                        break;

                    case "lastmonth":
                        var thisMonthStart = new DateTime(baseToday.Year, baseToday.Month, 1);
                        startDate = thisMonthStart.AddMonths(-1);
                        endDateExclusive = thisMonthStart;
                        break;

                    case "today":
                    default:
                        startDate = baseToday;
                        endDateExclusive = baseToday.AddDays(1);
                        break;
                }
            }

            var teamCode = HttpContext.Session.GetString("TeamCode");

            IQueryable<SalesFact> query = _context.VW_SALES_FACT
                .AsNoTracking()
                .Where(x =>
                    x.RefDate >= startDate &&
                    x.RefDate < endDateExclusive &&
                    x.LineTotal > 0 &&
                    !string.IsNullOrEmpty(x.ItemDescription)
                );

            if (isAll)
            {
                // no filter
            }
            else if (userType == "DIRECTOR")
            {
                var teamCodes = GetDirectorTeamCodes(userName, teamCode);
                query = query.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
            }
            else
            {
                query = query.Where(x => repCodes.Contains(x.SalesRepCode));
            }


            var grouped = query
                .GroupBy(x => x.ItemDescription)
                .Select(g => new
                {
                    Label = g.Key,
                    Value = metric == "qty"
                            ? (decimal)(g.Sum(x => (decimal?)x.Qty) ?? 0)
                            : Math.Round(g.Sum(x => (decimal?)x.LineTotal) ?? 0, 0)

                })
                .ToList();

            var grandTotal = grouped.Sum(x => x.Value);

            var top10 = grouped
                .OrderByDescending(x => x.Value)
                .Take(10)
                .ToList();

            var top10Total = top10.Sum(x => x.Value);
            var othersTotal = grandTotal - top10Total;

            var labels = top10.Select(x => x.Label).ToList();
            var values = top10.Select(x => x.Value).ToList();

            if (othersTotal > 0)
            {
                labels.Add("Others");
                values.Add(othersTotal);
            }

            var result = new PieChartDataVm
            {
                Labels = labels,
                Values = values
            };

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });

            return Json(result);
        }

        [HttpGet]
        public IActionResult DayWiseSalesData()
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            // DEV base date (replace with DateTime.Today later)
            var today = _dateProvider.Today;
            var yesterday = today.AddDays(-1);

            // We include yesterday + today
            var start = yesterday;               // 2025-11-19 00:00
            var endExclusive = today.AddDays(1); // 2025-11-21 00:00

            // business hours
            const int startHour = 8;
            const int endHour = 19; // inclusive (8..19)

            var cacheKey = $"daywise:{userName}:{userType}:{salesRepCode}:{today:yyyyMMdd}:{startHour}-{endHour}";
            if (_cache.TryGetValue(cacheKey, out DayWiseLineChartVm cached))
                return Json(cached);

            var repCodes = _salesAccessService.GetAccessibleRepCodes(userType, userName, salesRepCode);
            bool isAll = repCodes.Count == 1 && repCodes[0] == "__ALL__";

            // Base query: yesterday + today, business hours only
            var q = _context.VW_SALES_FACT
                .AsNoTracking()
                .Where(x =>
                    x.LineTotal > 0 &&
                    x.SysDateTime >= start &&
                    x.SysDateTime < endExclusive &&
                    x.SysDateTime.Hour >= startHour &&
                    x.SysDateTime.Hour <= endHour
                );

            if (!isAll)
            {
                if (!repCodes.Any())
                    return Json(new DayWiseLineChartVm());

                q = q.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            // Group by day + hour
            var grouped = q
                .GroupBy(x => new { Day = x.SysDateTime.Date, Hour = x.SysDateTime.Hour })
                .Select(g => new
                {
                    g.Key.Day,
                    g.Key.Hour,
                    Total = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                })
                .ToList();

            // Convert to dictionary for fast lookup
            var dict = grouped.ToDictionary(
                x => (Day: x.Day, Hour: x.Hour),
                x => x.Total
            );

            // Labels: 8 AM -> 7 PM
            var labels = Enumerable.Range(startHour, endHour - startHour + 1)
                .Select(h => DateTime.Today.Date.AddHours(h).ToString("h tt", CultureInfo.InvariantCulture))
                .ToList();

            var todayVals = new List<decimal>(labels.Count);
            var yestVals = new List<decimal>(labels.Count);

            for (int h = startHour; h <= endHour; h++)
            {
                todayVals.Add(dict.TryGetValue((today, h), out var t) ? t : 0);
                yestVals.Add(dict.TryGetValue((yesterday, h), out var y) ? y : 0);
            }

            var result = new DayWiseLineChartVm
            {
                Labels = labels,
                Today = todayVals,
                Yesterday = yestVals
            };

            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });

            return Json(result);
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

        [HttpGet]
        public IActionResult TeamMonthDetails(string locShort, string mode = "item", string? search = null)
        {
            var userName = HttpContext.Session.GetString("Username");
            var userType = HttpContext.Session.GetString("UserType");
            var salesRepCode = HttpContext.Session.GetString("SalesRepCode");
            var teamCode = HttpContext.Session.GetString("TeamCode");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userType))
                return RedirectToAction("Login", "Account");

            // -----------------------
            // Date range: this month
            // -----------------------
            var today = _dateProvider.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEndExclusive = monthStart.AddMonths(1);

            // -----------------------
            // Access filtering
            // -----------------------
            var repCodes = _salesAccessService.GetAccessibleRepCodes(userType, userName, salesRepCode);
            bool isAll = repCodes.Count == 1 && repCodes[0] == "__ALL__";

            IQueryable<SalesFact> salesQ = _context.VW_SALES_FACT.AsNoTracking()
                .Where(x => x.RefDate >= monthStart && x.RefDate < monthEndExclusive && x.LineTotal > 0);

            IQueryable<VwSalesReturnFact> retQ = _context.VW_SALES_RETURN_FACT.AsNoTracking()
                .Where(x => x.RefDate >= monthStart && x.RefDate < monthEndExclusive && x.LineTotal > 0);

            // Role filter (sales + returns)
            if (isAll)
            {
                // L006 / ALL => no role filter
            }
            else if (userType == "DIRECTOR")
            {
                var teamCodes = GetDirectorTeamCodes(userName, teamCode);
                if (!teamCodes.Any())
                {
                    return View("TeamMonthDetails", new TeamMonthDetailsViewModel
                    {
                        LocShort = locShort,
                        Mode = mode,
                        Search = search,
                        Suggestions = new List<string>(),
                        Columns = new List<string>(),
                        TableRows = new List<List<string>>()
                    });
                }

                // Director is team-based
                salesQ = salesQ.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
                retQ = retQ.Where(x => x.Pat_Name != null && teamCodes.Contains(x.Pat_Name));
            }
            else
            {
                if (!repCodes.Any())
                {
                    return View("TeamMonthDetails", new TeamMonthDetailsViewModel
                    {
                        LocShort = locShort,
                        Mode = mode,
                        Search = search,
                        Suggestions = new List<string>(),
                        Columns = new List<string>(),
                        TableRows = new List<List<string>>()
                    });
                }

                salesQ = salesQ.Where(x => repCodes.Contains(x.SalesRepCode));
                retQ = retQ.Where(x => repCodes.Contains(x.SalesRepCode));
            }

            // -----------------------
            // Fix the selected Team (LocShort)
            // -----------------------
            if (!string.IsNullOrWhiteSpace(locShort))
            {
                salesQ = salesQ.Where(x => x.LocShort == locShort);
                retQ = retQ.Where(x => x.LocShort == locShort);
            }

            // -----------------------
            // Search helpers
            // -----------------------
            string s = (search ?? "").Trim();
            bool HasSearch() => !string.IsNullOrWhiteSpace(s);

            // NOTE: Avoid ToLower on DB columns (better perf)
            // Most SQL Server collations are case-insensitive anyway.
            // If yours is case-sensitive, swap to EF.Functions.Like.
            var sLike = $"%{s}%";

            mode = (mode ?? "item").Trim().ToLowerInvariant();

            // =====================================================================
            // MODE: ITEM  (Item -> Net Qty/Net Amount)
            // =====================================================================
            if (mode == "item")
            {
                // Suggestions should be based on full dataset for this team/month,
                // NOT narrowed by the current search.
                var suggestionsQ = salesQ;

                if (HasSearch())
                {
                    salesQ = salesQ.Where(x => x.ItemDescription != null && x.ItemDescription.Contains(s));
                    retQ = retQ.Where(x => x.Description != null && x.Description.Contains(s));
                }

                var salesAgg = salesQ
                    .Where(x => x.ItemDescription != null && x.ItemDescription != "")
                    .GroupBy(x => x.ItemDescription)
                    .Select(g => new
                    {
                        Item = g.Key!,
                        Qty = g.Sum(x => (decimal?)x.Qty) ?? 0,
                        Amt = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                    })
                    .ToList();

                var retAgg = retQ
                    .Where(x => x.Description != null && x.Description != "")
                    .GroupBy(x => x.Description)
                    .Select(g => new
                    {
                        Item = g.Key!,
                        Qty = g.Sum(x => (decimal?)x.Qty) ?? 0,
                        Amt = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                    })
                    .ToList();

                var retDict = retAgg.ToDictionary(x => x.Item, x => x);

                var rows = new List<(string item, decimal qty, decimal amt)>();

                foreach (var x in salesAgg)
                {
                    retDict.TryGetValue(x.Item, out var r);
                    rows.Add((x.Item, x.Qty - (r?.Qty ?? 0), x.Amt - (r?.Amt ?? 0)));
                }

                // include items that appear only in returns (negative net)
                var salesSet = new HashSet<string>(salesAgg.Select(x => x.Item));
                foreach (var r in retAgg)
                {
                    if (!salesSet.Contains(r.Item))
                        rows.Add((r.Item, 0 - r.Qty, 0 - r.Amt));
                }

                rows = rows.OrderByDescending(x => x.amt).ToList();

                var itemSuggestions = suggestionsQ
                    .Where(x => x.ItemDescription != null && x.ItemDescription != "")
                    .Select(x => x.ItemDescription!)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(500)
                    .ToList();

                var vm = new TeamMonthDetailsViewModel
                {
                    LocShort = locShort,
                    Mode = mode,
                    Search = search,
                    Suggestions = itemSuggestions,

                    Columns = new List<string> { "Item", "Qty", "Net Total" },

                    NetTotal = rows.Sum(x => x.amt),
                    FooterQty = rows.Sum(x => x.qty),
                    FooterAmount = rows.Sum(x => x.amt),

                    TableRows = rows.Select(r => new List<string>
            {
                r.item,
                r.qty.ToString("N0"),
                r.amt.ToString("N0")
            }).ToList()
                };

                return View("TeamMonthDetails", vm);
            }

            // =====================================================================
            // MODE: REP  (Rep -> Item -> Net Amount)
            // =====================================================================
            if (mode == "rep")
            {
                var suggestionsQ = salesQ;

                if (HasSearch())
                {
                    salesQ = salesQ.Where(x => x.SalesRepName != null && x.SalesRepName.Contains(s));
                    retQ = retQ.Where(x => x.SalesRepName != null && x.SalesRepName.Contains(s));
                }

                var salesAgg = salesQ
                    .Where(x => x.SalesRepName != null && x.SalesRepName != ""
                             && x.ItemDescription != null && x.ItemDescription != "")
                    .GroupBy(x => new { x.SalesRepName, x.ItemDescription })
                    .Select(g => new
                    {
                        Rep = g.Key.SalesRepName!,
                        Item = g.Key.ItemDescription!,
                        Qty = g.Sum(x => (decimal?)x.Qty) ?? 0,
                        Amt = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                    })
                    .ToList();

                var retAgg = retQ
                    .Where(x => x.SalesRepName != null && x.SalesRepName != ""
                             && x.Description != null && x.Description != "")
                    .GroupBy(x => new { x.SalesRepName, Item = x.Description })
                    .Select(g => new
                    {
                        Rep = g.Key.SalesRepName!,
                        Item = g.Key.Item!,
                        Qty = g.Sum(x => (decimal?)x.Qty) ?? 0,
                        Amt = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                    })
                    .ToList();

                var retDict = retAgg.ToDictionary(x => (x.Rep, x.Item), x => x);

                var rows = new List<(string rep, string item, decimal qty, decimal amt)>();

                foreach (var x in salesAgg)
                {
                    retDict.TryGetValue((x.Rep, x.Item), out var r);
                    rows.Add((x.Rep, x.Item, x.Qty - (r?.Qty ?? 0), x.Amt - (r?.Amt ?? 0)));
                }

                // rows only in returns (negative net)
                var salesSet = new HashSet<(string Rep, string Item)>(salesAgg.Select(x => (x.Rep, x.Item)));
                foreach (var r in retAgg)
                {
                    if (!salesSet.Contains((r.Rep, r.Item)))
                        rows.Add((r.Rep, r.Item, 0 - r.Qty, 0 - r.Amt));
                }

                rows = rows
                    .OrderBy(x => x.rep)
                    .ThenByDescending(x => x.amt)
                    .ToList();

                var repSuggestions = suggestionsQ
                    .Where(x => x.SalesRepName != null && x.SalesRepName != "")
                    .Select(x => x.SalesRepName!)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(500)
                    .ToList();

                var vm = new TeamMonthDetailsViewModel
                {
                    LocShort = locShort,
                    Mode = mode,
                    Search = search,
                    Suggestions = repSuggestions,

                    Columns = new List<string> { "Rep", "Item", "Qty", "Net Total" },

                    NetTotal = rows.Sum(x => x.amt),
                    FooterQty = rows.Sum(x => x.qty),
                    FooterAmount = rows.Sum(x => x.amt),

                    TableRows = rows.Select(r => new List<string>
            {
                r.rep,
                r.item,
                r.qty.ToString("N0"),
                r.amt.ToString("N0")
            }).ToList()
                };

                return View("TeamMonthDetails", vm);
            }

            // =====================================================================
            // MODE: CUSTOMER  (Customer -> Item -> Rep -> Net Amount)
            // =====================================================================
            // default to customer if anything else
            {
                var suggestionsQ = salesQ;

                if (HasSearch())
                {
                    salesQ = salesQ.Where(x => x.CusName != null && x.CusName.Contains(s));
                    retQ = retQ.Where(x => x.CusName != null && x.CusName.Contains(s));
                }

                var salesAgg = salesQ
                    .Where(x => x.CusName != null && x.CusName != ""
                             && x.ItemDescription != null && x.ItemDescription != ""
                             && x.SalesRepName != null && x.SalesRepName != "")
                    .GroupBy(x => new { x.CusName, x.ItemDescription, x.SalesRepName })
                    .Select(g => new
                    {
                        Cus = g.Key.CusName!,
                        Item = g.Key.ItemDescription!,
                        Rep = g.Key.SalesRepName!,
                        Qty = g.Sum(x => (decimal?)x.Qty) ?? 0,
                        Amt = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                    })
                    .ToList();

                var retAgg = retQ
                    .Where(x => x.CusName != null && x.CusName != ""
                             && x.Description != null && x.Description != ""
                             && x.SalesRepName != null && x.SalesRepName != "")
                    .GroupBy(x => new { x.CusName, Item = x.Description, x.SalesRepName })
                    .Select(g => new
                    {
                        Cus = g.Key.CusName!,
                        Item = g.Key.Item!,
                        Rep = g.Key.SalesRepName!,
                        Qty = g.Sum(x => (decimal?)x.Qty) ?? 0,
                        Amt = g.Sum(x => (decimal?)x.LineTotal) ?? 0
                    })
                    .ToList();

                var retDict = retAgg.ToDictionary(x => (x.Cus, x.Item, x.Rep), x => x);

                var rows = new List<(string cus, string item, string rep, decimal qty, decimal amt)>();

                foreach (var x in salesAgg)
                {
                    retDict.TryGetValue((x.Cus, x.Item, x.Rep), out var r);
                    rows.Add((x.Cus, x.Item, x.Rep, x.Qty - (r?.Qty ?? 0), x.Amt - (r?.Amt ?? 0)));
                }

                var salesSet = new HashSet<(string Cus, string Item, string Rep)>(salesAgg.Select(x => (x.Cus, x.Item, x.Rep)));
                foreach (var r in retAgg)
                {
                    if (!salesSet.Contains((r.Cus, r.Item, r.Rep)))
                        rows.Add((r.Cus, r.Item, r.Rep, 0 - r.Qty, 0 - r.Amt));
                }

                rows = rows
                    .OrderBy(x => x.cus)
                    .ThenByDescending(x => x.amt)
                    .ToList();

                var cusSuggestions = suggestionsQ
                    .Where(x => x.CusName != null && x.CusName != "")
                    .Select(x => x.CusName!)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(500)
                    .ToList();

                var vm = new TeamMonthDetailsViewModel
                {
                    LocShort = locShort,
                    Mode = "customer",
                    Search = search,
                    Suggestions = cusSuggestions,

                    Columns = new List<string> { "Customer", "Item", "Rep", "Qty", "Net Total" },

                    NetTotal = rows.Sum(x => x.amt),
                    FooterQty = rows.Sum(x => x.qty),
                    FooterAmount = rows.Sum(x => x.amt),

                    TableRows = rows.Select(r => new List<string>
            {
                r.cus,
                r.item,
                r.rep,
                r.qty.ToString("N0"),
                r.amt.ToString("N0")
            }).ToList()
                };

                return View("TeamMonthDetails", vm);
            }
        }



    }
}
