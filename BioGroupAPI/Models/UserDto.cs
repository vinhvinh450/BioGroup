namespace BioGroupAPI.Models;

public class UserDto
{
    public int      Id        { get; set; }
    public string   Email     { get; set; } = string.Empty;
    public string   Name      { get; set; } = string.Empty;
    public string   Phone     { get; set; } = string.Empty;
    public string   Address   { get; set; } = string.Empty;
    public string   Role      { get; set; } = "User";
    public bool     IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}