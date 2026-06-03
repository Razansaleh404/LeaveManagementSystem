using LeaveManagement.API.DTOs;

namespace LeaveManagement.API.Services;

public interface ILeaveRequestService
{
    Task<List<LeaveRequestDto>> GetAllAsync();
    Task<LeaveRequestDto?> GetByIdAsync(int id);
    Task<List<LeaveRequestDto>> FilterAsync(string? status, DateTime? fromDate, DateTime? toDate);
    Task<List<LeaveRequestDto>> GetPendingAsync();
    Task<(bool Success, string? Error, LeaveRequestDto? Request)> CreateAsync(LeaveRequestCreateDto dto);
    Task<(bool Success, string? Error, LeaveRequestDto? Request)> ApproveAsync(int id, LeaveDecisionDto dto);
    Task<(bool Success, string? Error, LeaveRequestDto? Request)> RejectAsync(int id, LeaveDecisionDto dto);
}
