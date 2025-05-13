using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using APBD_example_test1_2025.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_example_test1_2025.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> getBookingsAsync(int id)
    {
        try
        {
            var result = await _bookingService.GetBookingById(id);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    [HttpPost]
    public async Task<IActionResult> makeReservationBooking(ReservationBookingDTO reservationBookingDTO)
    {
       await _bookingService.MakeReservation(reservationBookingDTO);
       return Ok("Utworzono pomyslnie");
    }
}