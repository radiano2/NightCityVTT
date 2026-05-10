using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMongoCollection<UserDocument> _users;

    public UserController(IMongoClient mongo, IConfiguration config)
    {
        var db = mongo.GetDatabase(config["MongoDB:DatabaseName"] ?? "NightCityVTT");
        _users = db.GetCollection<UserDocument>("Users");
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists()
    {
        var count = await _users.CountDocumentsAsync(_ => true);
        return Ok(count > 0);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var users = await _users.Find(_ => true).SortBy(u => u.Nickname).ToListAsync();
        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nickname) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Nickname and password required.");

        var exists = await _users.Find(u => u.Nickname == req.Nickname.Trim()).AnyAsync();
        if (exists) return Conflict("Nickname already taken.");

        var doc = new UserDocument
        {
            Nickname     = req.Nickname.Trim(),
            PasswordHash = Hash(req.Password),
            Role         = req.Role
        };
        await _users.InsertOneAsync(doc);
        return Ok(doc.ToDto());
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var hash = Hash(req.Password);
        var user = await _users
            .Find(u => u.Nickname == req.Nickname && u.PasswordHash == hash)
            .FirstOrDefaultAsync();
        return user is null ? Unauthorized("Invalid credentials.") : Ok(user.ToDto());
    }

    private static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
