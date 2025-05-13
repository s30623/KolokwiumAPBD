using APBD_example_test1_2025.Models.DTOs;

namespace APBD_example_test1_2025.Services;

public interface IBookingService
{
    Task<BookingDTO> GetBookingById(int id);
    Task MakeReservation(ReservationBookingDTO reservationBookingDTO);
}