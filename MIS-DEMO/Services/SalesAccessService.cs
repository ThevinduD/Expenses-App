using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Data;

namespace MIS_DEMO.Services
{
    public class SalesAccessService
    {
        private readonly AppDbContext _context;

        public SalesAccessService(AppDbContext context)
        {
            _context = context;
        }

        public List<string> GetAccessibleRepCodes(
            string userType,
            string userName,
            string? salesRepCode)
        {
            var repCodes = new List<string>();

            // REP
            if (userType == "REP" && !string.IsNullOrEmpty(salesRepCode))
            {
                repCodes.Add(salesRepCode);
                return repCodes;
            }

            // ASM / SM
            if (userType == "ASM")
            {
                // Check if SM
                var isSM = _context.WKF_MAP_SM_ASM
                    .AsNoTracking()
                    .Any(x => x.UserNameSM == userName);

                if (isSM)
                {
                    // 1. Get ASMs under SM
                    var asmUserNames = _context.WKF_MAP_SM_ASM
                        .AsNoTracking()
                        .Where(x => x.UserNameSM == userName)
                        .Select(x => x.UserNameASM)
                        .ToList();

                    // 2. Get REP codes under ASMs
                    repCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => asmUserNames.Contains(x.UserName))
                        .Select(x => x.SalesRepCode)
                        .ToList();
                }
                else
                {
                    // Normal ASM → REPs under him
                    repCodes = _context.WKF_MAP_REP_ASM
                        .AsNoTracking()
                        .Where(x => x.UserName == userName)
                        .Select(x => x.SalesRepCode)
                        .ToList();
                }

                // 3. Add own sales if exists
                if (!string.IsNullOrEmpty(salesRepCode) &&
                    !repCodes.Contains(salesRepCode))
                {
                    repCodes.Add(salesRepCode);
                }
            }

            return repCodes;
        }
    }
}

