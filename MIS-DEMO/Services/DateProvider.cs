using Microsoft.Extensions.Configuration;

namespace MIS_DEMO.Services
{
    public class DateProvider : IDateProvider
    {
        private readonly IConfiguration _config;

        public DateProvider(IConfiguration config)
        {
            _config = config;
        }

        public DateTime Today
        {
            get
            {
                // If DevDate is set, use it
                var devDate = _config["AppSettings:DevTodayDate"];
                if (!string.IsNullOrEmpty(devDate) &&
                    DateTime.TryParse(devDate, out var parsed))
                {
                    return parsed.Date;
                }

                // Otherwise real today
                return DateTime.Today;
            }
        }
    }
}
