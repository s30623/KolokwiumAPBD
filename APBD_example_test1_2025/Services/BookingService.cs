using System.Data.Common;
using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_example_test1_2025.Services;

public class BookingService : IBookingService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;";

    public async Task<BookingDTO> GetBookingById(int id)
    {
        var query = @"SELECT b.date,g.first_name AS Gfirst_name, g.last_name as Glast_name, g.date_of_birth, 
       e.first_name AS Efirst_name, e.last_name AS Elast_name, e.employee_number, a.name, a.price, ba.amount 
       FROM Booking b 
           JOIN Guest g ON g.guest_id = b.guest_id 
           JOIN Employee e ON b.employee_id = e.employee_id 
           JOIN Booking_Attraction ba ON ba.booking_id = b.booking_id 
           JOIN Attraction a ON ba.attraction_id = a.attraction_id 
       WHERE b.booking_id = @bookingId";

        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();

        command.Parameters.AddWithValue("@bookingId", id);
        var reader = await command.ExecuteReaderAsync();

        BookingDTO bookingDTO = null;
        while (await reader.ReadAsync())
        {
            if (bookingDTO is null)
            {
                bookingDTO = new BookingDTO
                {
                    date = (DateTime)reader["date"],
                    guest = new GuestDTO
                    {
                        firstName = reader["Gfirst_name"].ToString(),
                        lastName = reader["Glast_name"].ToString(),
                        dateOfBirth = (DateTime)reader["date_of_birth"],
                    },
                    employee = new EmployeeDTO
                    {
                        firstName = reader["Efirst_name"].ToString(),
                        lastName = reader["Elast_name"].ToString(),
                        employeeNumber = reader["employee_number"].ToString(),
                    },
                    attractions = new List<AttractionsDTO>()
                };
                bookingDTO.attractions.Add(new AttractionsDTO()
                {
                    amount = (int)reader["amount"],
                    name = reader["name"].ToString(),
                    price = (decimal)reader["price"],
                });
            }
        }

        if (bookingDTO is null)
        {
            throw new NotFoundException("Booking not found");
        }
        return bookingDTO;
    }

    public async Task MakeReservation(ReservationBookingDTO reservationBookingDTO)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            
            //check bookingId exists
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Booking WHERE booking_id = @IdBooking;";
            command.Parameters.AddWithValue("@IdBooking", reservationBookingDTO.bookingId);
                
            var bookingIdRes = await command.ExecuteScalarAsync();
            if(bookingIdRes is not null)
                throw new NotFoundException($"Reservation with ID - {reservationBookingDTO.bookingId} - exists.");
            
            command.Parameters.Clear();
            
            
            //check guset exists
            command.CommandText = "SELECT 1 FROM Guest WHERE guest_id = @IdGuest;";
            command.Parameters.AddWithValue("@IdGuest", reservationBookingDTO.guestId);
                
            var guestIdRes = await command.ExecuteScalarAsync();
            if(guestIdRes is null)
                throw new NotFoundException($"Guest with ID - {reservationBookingDTO.guestId} - does not exists.");
            
            command.Parameters.Clear();
            //check if employee exists
            
            command.CommandText = "SELECT employee_id FROM Employee WHERE employee_number = @IdEmployee;";
            command.Parameters.AddWithValue("@IdEmployee", reservationBookingDTO.employeeNumber);
                
            var employeeIdRes = await command.ExecuteScalarAsync();
            if(employeeIdRes is null)
                throw new NotFoundException($"Employee with ID - {reservationBookingDTO.employeeNumber} - does not exists.");
            
            command.Parameters.Clear();
            
            //check if attraction exsist
            foreach (var attraction in reservationBookingDTO.attractions)
            {
                command.CommandText = "SELECT 1 FROM Attraction WHERE name = @NameAttraction;";
                command.Parameters.AddWithValue("@NameAttraction", attraction.name);
                
                var attractionNameRes = await command.ExecuteScalarAsync();
                if(attractionNameRes is null)
                    throw new NotFoundException($"Attraction - {attraction.name} - does not exists.");
            
                command.Parameters.Clear();
            }
            
            
            command.CommandText = 
                @"INSERT INTO Booking
            VALUES(@BookingId, @GuestId, @EmployeeId, @Date);";
        
            command.Parameters.AddWithValue("@BookingId", reservationBookingDTO.bookingId);
            command.Parameters.AddWithValue("@GuestId", reservationBookingDTO.guestId);
            command.Parameters.AddWithValue("@EmployeeId", employeeIdRes);
            command.Parameters.AddWithValue("@Date", DateTime.Now);

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new ConflictException("A rental with the same ID already exists.");
            }
            
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}