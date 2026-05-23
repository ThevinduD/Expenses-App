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


            return View(model);
        }

        // ==========================================
        // PRIVATE HELPER METHODS 
        // ==========================================

        private IQueryable<T> ApplySalesRoleFilter<T>(IQueryable<T> query, string userName, string userType, string salesRepCode, string teamCode) where T : class
        {
            // 1. REP (Stays exactly the same)
            if (userType == "REP")
            {
                return query.Where(e => EF.Property<string>(e, "SalesRepCode") == salesRepCode);
            }

            // 2. ADMIN (L006 sees everything)
            if (userType == "DIRECTOR" && teamCode == "L006")
            {
                return query;
            }

            // 3. EVERYONE ELSE (SM, ASM, DIRECTOR Not L006)
            // They all now use the new GetAllowedRepCodes logic!
            if (userType == "ASM" || userType == "SM" || userType == "OTHER" || (userType == "DIRECTOR" && teamCode != "L006"))
            {
                var repCodes = GetAllowedRepCodes(userName, userType);

                // Include their direct SalesRepCode from session just in case
                if (!string.IsNullOrEmpty(salesRepCode) && !repCodes.Contains(salesRepCode))
                {
                    repCodes.Add(salesRepCode);
                }

                return query.Where(e => repCodes.Contains(EF.Property<string>(e, "SalesRepCode")));
            }

            return query;
        }

        private IQueryable<CustomerInvoiceMain> ApplyInvoiceRoleFilter(IQueryable<CustomerInvoiceMain> query, string userName, string userType, string salesRepCode, string teamCode)
        {
            // 1. REP
            if (userType == "REP")
            {
                return query.Where(x => x.SalesRepCode == salesRepCode);
            }

            // 2. ADMIN (L006)
            if (userType == "DIRECTOR" && teamCode == "L006")
            {
                return query;
            }

            // 3. EVERYONE ELSE (SM, ASM, DIRECTOR Not L006)
            if (userType == "ASM" || userType == "SM" || userType == "OTHER" || (userType == "DIRECTOR" && teamCode != "L006"))
            {
                var repCodes = GetAllowedRepCodes(userName, userType);
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

        private IQueryable<LoginLog> ApplyLoginRoleFilter(IQueryable<LoginLog> query, string userName, string userType, string teamCode)
        {
            // 1. REP: Can only see their own logins
            if (userType == "REP")
            {
                return query.Where(x => x.Username == userName);
            }

            // 2. ADMIN (L006): Sees everyone
            if (userType == "DIRECTOR" && teamCode == "L006")
            {
                return query;
            }

            // 3. EVERYONE ELSE (SM, ASM, DIRECTOR Not L006)
            if (userType == "ASM" || userType == "SM" || userType == "OTHER" || (userType == "DIRECTOR" && teamCode != "L006"))
            {
                // ---> THE FIX: We pass BOTH userName and userType here <---
                var allowedRepCodes = GetAllowedRepCodes(userName, userType);

                // Convert the allowed SalesRepCodes into Usernames
                var allowedUsernames = _context.WKF_USER_REP_MAP.AsNoTracking()
                    .Where(x => allowedRepCodes.Contains(x.SalesRepCode))
                    .Select(x => x.UserName)
                    .ToList();

                allowedUsernames.Add(userName); // Include their own username so they can see their own logins

                return query.Where(x => allowedUsernames.Contains(x.Username));
            }

            return query;
        }

        private List<string> GetAllowedRepCodes(string userName, string userType)
        {

            var cachedReps = HttpContext.Session.GetString("MyAllowedReps");
            if (!string.IsNullOrEmpty(cachedReps))
            {
                return cachedReps.Split(',').ToList(); // Instantly return from memory!
            }

            var repCodes = new List<string>();

            if (userType == "REP")
            {
                return repCodes;
            }

            else if (userType == "ASM" || userType == "OTHER")
            {
                // First, check if this user is actually an SM by looking them up in the mapping table
                var assignedASMs = _context.WKF_MAP_SM_ASM
                    .Where(x => x.UserNameSM == userName)
                    .Select(x => x.UserNameASM)
                    .ToList();

                if (assignedASMs.Any())
                {
                    // ---> THEY ARE AN SM <---
                    repCodes = _context.WKF_MAP_REP_ASM_MIS
                        .Where(x => assignedASMs.Contains(x.UserName) || x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    // ---> THEY ARE A NORMAL ASM (or OTHER) <---
                    repCodes = _context.WKF_MAP_REP_ASM_MIS
                        .Where(x => x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .Distinct()
                        .ToList();
                }
            }

            else if (userType == "DIRECTOR")
            {
                var assignedUserNames = _context.WKF_MAP_ASM_DIR
                    .Where(x => x.UserNameDir == userName)
                    .Select(x => x.UserNameAsm)
                    .ToList();

                repCodes = _context.WKF_MAP_REP_ASM_MIS
                    .Where(x => assignedUserNames.Contains(x.UserName))
                    .Select(x => x.SalesRepCode)
                    .Distinct()
                    .ToList();
            }

            // Save the final list to the Session so we never hit the DB again for this user!
            if (repCodes.Any())
            {
                HttpContext.Session.SetString("MyAllowedReps", string.Join(",", repCodes));
            }

            return repCodes;
        }

        
    }
}