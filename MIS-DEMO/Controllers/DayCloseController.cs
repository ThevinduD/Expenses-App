using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MIS_DEMO.Controllers
{
    public class DayCloseController : Controller
    {
        private readonly AppDbContext _context;

        public DayCloseController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult DayClose()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveDayClose([FromForm] DayCloseMaster dayClose)
        {
            try
            {
                var currentUser = HttpContext.Session.GetString("Username");

                if (string.IsNullOrEmpty(currentUser))
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }

                dayClose.Username = currentUser;
                dayClose.RepCode = HttpContext.Session.GetString("SalesRepCode");

                // RULE 1: Check if an itinerary exists for this date and this user
                var hasItinerary = await _context.ItineraryMasters
                    .AnyAsync(i => i.Username == currentUser && i.ItineraryDate.Date == dayClose.CloseDate.Date);

                if (!hasItinerary)
                {
                    return Json(new { success = false, message = $"No itinerary found for {dayClose.CloseDate:yyyy-MM-dd}. You cannot submit a day close without a planned itinerary." });
                }

                // RULE 2: Check if a day close already exists for this date and this user
                var existingClose = await _context.DayCloseMasters
                    .AnyAsync(d => d.Username == currentUser && d.CloseDate.Date == dayClose.CloseDate.Date);

                if (existingClose)
                {
                    return Json(new { success = false, message = $"You have already submitted a Day Close for {dayClose.CloseDate:yyyy-MM-dd}." });
                }

                // If both rules pass, save the record
                _context.DayCloseMasters.Add(dayClose);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Day Close saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving Day Close: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetItineraryForDate(DateTime date)
        {
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser))
            {
                return Json(new { success = false, message = "Session expired." });
            }

            // Find the itinerary for this user on this specific date
            var itinerary = await _context.ItineraryMasters
                .FirstOrDefaultAsync(i => i.Username == currentUser && i.ItineraryDate.Date == date.Date);

            if (itinerary == null)
            {
                return Json(new { success = false, message = "Itinerary not found. Please enter an itinerary first." });
            }

            // Return the type (Work/Leave) and the Remarks (the cities/reasons)
            return Json(new
            {
                success = true,
                itineraryType = itinerary.ItineraryType,
                remark = itinerary.Remark
            });
        }
    }
}