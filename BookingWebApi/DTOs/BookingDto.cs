namespace BookingWebApi.DTOs;
public record BookingDto(int BookingId, string RoomCode, DateTime BookingDate, string Status, List<int> SlotNumbers);
