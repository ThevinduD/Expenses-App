using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS_DEMO.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CHEQUE",
                columns: table => new
                {
                    ChequeRefNO = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChequeRefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dr = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cr = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Deposit_Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isRealized = table.Column<bool>(type: "bit", nullable: true),
                    Deposit_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Returned_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChequeStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptVoucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RealizedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Realized_Date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CHEQUE", x => x.ChequeRefNO);
                });

            migrationBuilder.CreateTable(
                name: "CUS_CATEGORY",
                columns: table => new
                {
                    CusCategoryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CusCategoryName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUS_CATEGORY", x => x.CusCategoryCode);
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMER_INVOICE_MAIN",
                columns: table => new
                {
                    InvoDocNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ComCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LocCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SalesRepCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    isFinalDelivery = table.Column<bool>(type: "bit", nullable: true),
                    Cancel = table.Column<bool>(type: "bit", nullable: false),
                    Pat_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMER_INVOICE_MAIN", x => new { x.InvoDocNo, x.ComCode, x.LocCode });
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMER_OUTSTANDING",
                columns: table => new
                {
                    ComCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuePayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Bill_Com_Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManualNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isExcelUpload = table.Column<bool>(type: "bit", nullable: true),
                    isSystmUpload = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMER_PAYMENT",
                columns: table => new
                {
                    PaymentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PayType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeRefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardRefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cancel = table.Column<bool>(type: "bit", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CusCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMER_PAYMENT_TEMP",
                columns: table => new
                {
                    PaymentNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PayType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeRefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardRefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cancel = table.Column<bool>(type: "bit", nullable: true),
                    isDeposited = table.Column<bool>(type: "bit", nullable: true),
                    CusCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "DIR_SUP_MAP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserNameDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DIR_SUP_MAP", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DIR_TEAM_MAP",
                columns: table => new
                {
                    UserNameDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ExpenseMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TranNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpenseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ITEM_PRICE",
                columns: table => new
                {
                    ItemRefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type_Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    S_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    M_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "LINKS",
                columns: table => new
                {
                    LinkName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LINKS", x => x.LinkName);
                });

            migrationBuilder.CreateTable(
                name: "LOGIN_LOGS",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RealName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOGIN_LOGS", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "mascredittype",
                columns: table => new
                {
                    credittypeky = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    creditdays = table.Column<int>(type: "int", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    inactive = table.Column<bool>(type: "bit", nullable: false),
                    createdatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sessionid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mascredittype", x => x.credittypeky);
                });

            migrationBuilder.CreateTable(
                name: "MIS_DEFAULT_CONFIG",
                columns: table => new
                {
                    OutstandInvType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgingDefaultDays = table.Column<int>(type: "int", nullable: true),
                    PdCheq = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectionFreeze = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CollectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NonDelDefDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "NON_DELIVERED_DAYS",
                columns: table => new
                {
                    Days = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "OUTSTANDING_DAYS",
                columns: table => new
                {
                    Days = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "PartnerDetails",
                columns: table => new
                {
                    Pcode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    refno = table.Column<int>(type: "int", nullable: false),
                    ptype = table.Column<bool>(type: "bit", nullable: false),
                    AltCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustCat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FaxNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Web = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Merital = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DOW = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NICno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BRNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntroBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntroDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPersonDesig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPersonMob = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoMembers = table.Column<int>(type: "int", nullable: false),
                    LoyaltyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsParentUser = table.Column<bool>(type: "bit", nullable: false),
                    PointPrecntage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvaPoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KeyParentPcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentPcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    confirm = table.Column<bool>(type: "bit", nullable: false),
                    availabledispoint = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CusCategoryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxSaleslimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCompanyCCUser = table.Column<bool>(type: "bit", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsUpload = table.Column<bool>(type: "bit", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isNonTrading = table.Column<bool>(type: "bit", nullable: true),
                    Area = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerDetails", x => x.Pcode);
                });

            migrationBuilder.CreateTable(
                name: "RECEIPT",
                columns: table => new
                {
                    ReceiptNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalPaidAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RECEIPT", x => x.ReceiptNo);
                });

            migrationBuilder.CreateTable(
                name: "RECEIPT_PAY_INFO",
                columns: table => new
                {
                    ComCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChequeCardNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "SALES_REP",
                columns: table => new
                {
                    SalesRepCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SalesRepName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALES_REP", x => x.SalesRepCode);
                });

            migrationBuilder.CreateTable(
                name: "SUPPLIER_ASM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ASMCODE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SUPCODE = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SUPPLIER_ASM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TARGET_MAIN",
                columns: table => new
                {
                    TranNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Frequence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusMonth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TARGET_MAIN", x => x.TranNo);
                });

            migrationBuilder.CreateTable(
                name: "TARGET_MONTHS_REP_SPECIAL",
                columns: table => new
                {
                    TranNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserNameAsm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesRepCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthNo = table.Column<int>(type: "int", nullable: true),
                    TargetActual = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthNameTarget = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetRefNo = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TEAM_MIS",
                columns: table => new
                {
                    LocCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LocShort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TEAM_MIS", x => x.LocCode);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "VW_SALES_FACT",
                columns: table => new
                {
                    CusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoDocNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pat_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocShort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoldPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesRepCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesRepName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_SALES_RETURN_FACT",
                columns: table => new
                {
                    CusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RtnDocNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoDocNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pat_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocShort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesRepCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesRepName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "VW_STOCK_TEAM_VALUE",
                columns: table => new
                {
                    TeamCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemRefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StockQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDays = table.Column<int>(type: "int", nullable: true),
                    ShipmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgingDays = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WKF_MAP_ASM_DIR",
                columns: table => new
                {
                    UserNameDir = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserNameAsm = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WKF_MAP_REP_ASM",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesRepCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WKF_MAP_REP_ASM_MIS",
                columns: table => new
                {
                    SalesRepCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WKF_MAP_REP_ASM_MIS", x => x.SalesRepCode);
                });

            migrationBuilder.CreateTable(
                name: "WKF_MAP_SM_ASM",
                columns: table => new
                {
                    UserNameSM = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserNameASM = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "WKF_USER_REP_MAP",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesRepCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeamCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CHEQUE");

            migrationBuilder.DropTable(
                name: "CUS_CATEGORY");

            migrationBuilder.DropTable(
                name: "CUSTOMER_INVOICE_MAIN");

            migrationBuilder.DropTable(
                name: "CUSTOMER_OUTSTANDING");

            migrationBuilder.DropTable(
                name: "CUSTOMER_PAYMENT");

            migrationBuilder.DropTable(
                name: "CUSTOMER_PAYMENT_TEMP");

            migrationBuilder.DropTable(
                name: "DIR_SUP_MAP");

            migrationBuilder.DropTable(
                name: "DIR_TEAM_MAP");

            migrationBuilder.DropTable(
                name: "ExpenseMaster");

            migrationBuilder.DropTable(
                name: "ITEM_PRICE");

            migrationBuilder.DropTable(
                name: "LINKS");

            migrationBuilder.DropTable(
                name: "LOGIN_LOGS");

            migrationBuilder.DropTable(
                name: "mascredittype");

            migrationBuilder.DropTable(
                name: "MIS_DEFAULT_CONFIG");

            migrationBuilder.DropTable(
                name: "NON_DELIVERED_DAYS");

            migrationBuilder.DropTable(
                name: "OUTSTANDING_DAYS");

            migrationBuilder.DropTable(
                name: "PartnerDetails");

            migrationBuilder.DropTable(
                name: "RECEIPT");

            migrationBuilder.DropTable(
                name: "RECEIPT_PAY_INFO");

            migrationBuilder.DropTable(
                name: "SALES_REP");

            migrationBuilder.DropTable(
                name: "SUPPLIER_ASM");

            migrationBuilder.DropTable(
                name: "TARGET_MAIN");

            migrationBuilder.DropTable(
                name: "TARGET_MONTHS_REP_SPECIAL");

            migrationBuilder.DropTable(
                name: "TEAM_MIS");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "VW_SALES_FACT");

            migrationBuilder.DropTable(
                name: "VW_SALES_RETURN_FACT");

            migrationBuilder.DropTable(
                name: "VW_STOCK_TEAM_VALUE");

            migrationBuilder.DropTable(
                name: "WKF_MAP_ASM_DIR");

            migrationBuilder.DropTable(
                name: "WKF_MAP_REP_ASM");

            migrationBuilder.DropTable(
                name: "WKF_MAP_REP_ASM_MIS");

            migrationBuilder.DropTable(
                name: "WKF_MAP_SM_ASM");

            migrationBuilder.DropTable(
                name: "WKF_USER_REP_MAP");
        }
    }
}
