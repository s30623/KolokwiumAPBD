namespace APBD_example_test1_2025.Models.DTOs;

public class ReservationBookingDTO
{
    public int bookingId { get; set; }
    public int guestId { get; set; }
    public string employeeNumber { get; set; }
    public List<AttractionsDTO> attractions { get; set; }
    
}