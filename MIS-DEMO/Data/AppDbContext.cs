using Microsoft.EntityFrameworkCore;
using MIS_DEMO.Models;
using System.Collections.Generic;

namespace MIS_DEMO.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<Users> USERS { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().HasKey(x => x.UserName);
            modelBuilder.Entity<SalesFact>().HasNoKey();
            modelBuilder.Entity<VwSalesReturnFact>().HasNoKey();
            modelBuilder.Entity<CustomerInvoiceMain>()
                        .HasKey(x => new { x.InvoDocNo, x.ComCode, x.LocCode });
            modelBuilder.Entity<TeamMis>().HasKey(x => x.LocCode);

        }

        public DbSet<UserRepMap> WKF_USER_REP_MAP { get; set; }
        public DbSet<SalesFact> VW_SALES_FACT { get; set; }
        public DbSet<VwSalesReturnFact> VW_SALES_RETURN_FACT { get; set; }
        public DbSet<WKF_MAP_REP_ASM> WKF_MAP_REP_ASM { get; set; }
        public DbSet<WKF_MAP_SM_ASM> WKF_MAP_SM_ASM { get; set; }
        public DbSet<DirTeamMap> DIR_TEAM_MAP { get; set; }
        public DbSet<WkfMapAsmDir> WKF_MAP_ASM_DIR { get; set; }
        public DbSet<CustomerInvoiceMain> CUSTOMER_INVOICE_MAIN { get; set; }
        public DbSet<TeamMis> TEAM_MIS { get; set; }
        public DbSet<StockTeamValue> VW_STOCK_TEAM_VALUE { get; set; }
        public DbSet<SupplierAsm> SUPPLIER_ASM { get; set; }
        public DbSet<DirSupMap> DIR_SUP_MAP { get; set; }
        public DbSet<TargetMonthsRepSpecial> TARGET_MONTHS_REP_SPECIAL { get; set; }
    }
}
