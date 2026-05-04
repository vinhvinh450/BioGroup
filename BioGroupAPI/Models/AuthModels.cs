using System.ComponentModel.DataAnnotations.Schema;

namespace BioGroupAPI.Models;
public class UpdateProfileRequest
{
    public string? Name    { get; set; }
    public string? Phone   { get; set; }
    public string? Address { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword     { get; set; } = string.Empty;
}