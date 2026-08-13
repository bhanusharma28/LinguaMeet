using System.Security.Claims;
using LinguaMeet.Api.DTO;
using LinguaMeet.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinguaMeet.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(IAuthService s) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto d)
    {
        await s.RegisterAsync(d);
        return StatusCode(201, new { success = true });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto d)
    {
        var x = await s.LoginAsync(d);
        return x == null
            ? Unauthorized(new { success = false, message = "Invalid email or password." })
            : Ok(x);
    }
}

[ApiController, Authorize, Route("api/user")]
public class UserController(IUserService s) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me() => Ok(await s.GetAsync(Uid()));

    int Uid() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController, Authorize, Route("api/meetings")]
public class MeetingController(IMeetingService s) : ControllerBase
{
    int Uid() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create() => Ok(await s.CreateAsync(Uid()));

    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code)
    {
        var x = await s.GetAsync(code);
        return x == null ? NotFound() : Ok(x);
    }

    [HttpPost("{code}/join")]
    public async Task<IActionResult> Join(string code, JoinMeetingDto d)
    {
        var x = await s.JoinAsync(code, Uid(), d.SelectedLanguage);
        return x == null ? NotFound() : Ok(x);
    }

    [HttpPost("{code}/leave")]
    public async Task<IActionResult> Leave(string code) =>
        await s.LeaveAsync(code, Uid()) ? Ok() : NotFound();

    [HttpGet("history")]
    public async Task<IActionResult> History() => Ok(await s.HistoryAsync(Uid()));
}

[ApiController, Authorize, Route("api/transcripts")]
public class TranscriptController(ITranscriptService s) : ControllerBase
{
    int Uid() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("meeting/{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var x = await s.GetAsync(id, Uid());
        return x == null ? Forbid() : Ok(x);
    }

    [HttpPost("meeting/{id:int}")]
    public async Task<IActionResult> Save(int id, SaveTranscriptDto d) =>
        Ok(await s.SaveAsync(id, Uid(), d));
}
