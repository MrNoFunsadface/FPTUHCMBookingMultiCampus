namespace BookingWebApi.DTOs;
public record CreateBookingRequestDto(int RoomId, List<int> SlotIds, DateTime BookingDate);
