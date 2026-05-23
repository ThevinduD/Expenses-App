using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MIS_DEMO.Data;
using MIS_DEMO.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Make sure you have this to use FirstOrDefaultAsync!

namespace MIS_DEMO.Controllers
{
    public class ItineraryController : Controller
    {
        private readonly AppDbContext _context;

        public ItineraryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Itinerary()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveItinerary([FromForm] ItineraryMaster itinerary)
        {
            try
            {
                // Securely grab the logged-in user details from the Session
                var currentUser = HttpContext.Session.GetString("Username");
                itinerary.Username = currentUser;
                itinerary.RepCode = HttpContext.Session.GetString("SalesRepCode");

                // Optional failsafe: reject if session is dead
                if (string.IsNullOrEmpty(itinerary.Username))
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }

                // --- NEW: Duplicate Date Validation ---
                // Check if this user already has a record for the chosen date
                var existingItinerary = await _context.ItineraryMasters
                    .FirstOrDefaultAsync(i => i.Username == currentUser && i.ItineraryDate.Date == itinerary.ItineraryDate.Date);

                if (existingItinerary != null)
                {
                    // If a record is found, stop and return an error message to the frontend
                    return Json(new
                    {
                        success = false,
                        message = $"You have already submitted an itinerary ({existingItinerary.ItineraryType}) for {itinerary.ItineraryDate.ToString("yyyy-MM-dd")}. You cannot submit multiple itineraries for the same day."
                    });
                }
                // --------------------------------------

                _context.ItineraryMasters.Add(itinerary);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Itinerary saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving itinerary: " + ex.Message });
            }
        }
    }
}