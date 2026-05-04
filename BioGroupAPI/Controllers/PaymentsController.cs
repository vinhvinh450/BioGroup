using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BioGroupAPI.Data;
using BioGroupAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioGroupAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
private readonly BioGroupContext _context;
private readonly IConfiguration _config;

public PaymentsController(BioGroupContext context, IConfiguration config)
{
    _context = context;
    _config = config;
}




}
