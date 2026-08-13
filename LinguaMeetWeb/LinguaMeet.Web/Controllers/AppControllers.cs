using LinguaMeet.Web.Models;
using LinguaMeet.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinguaMeet.Web.Controllers;

public class AuthController(ApiClientService api) : Controller
{
    [HttpGet("/auth/login")]
    public IActionResult Login() => View();

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var (r, e) = await api.SendAsync<LoginResult>(HttpMethod.Post, "api/auth/login", m);
        if (r == null)
        {
            ModelState.AddModelError("", e!);
            return View(m);
        }
        HttpContext.Session.SetString("Token", r.Token);
        HttpContext.Session.SetString("Name", r.DisplayName);
        HttpContext.Session.SetString("Language", r.PreferredLanguage);
        HttpContext.Session.SetInt32("UserId", r.UserId);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet("/auth/register")]
    public IActionResult Register() => View();

    [HttpPost("/auth/register")]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var (_, e) = await api.SendAsync<object>(
            HttpMethod.Post,
            "api/auth/register",
            new
            {
                m.DisplayName,
                m.Email,
                m.Password,
                m.PreferredLanguage,
            }
        );
        if (e != null)
        {
            ModelState.AddModelError("", e);
            return View(m);
        }
        TempData["Message"] = "Account created. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost("/auth/logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}

public class DashboardController(ApiClientService api) : Controller
{
    [HttpGet("/dashboard")]
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Token") == null)
            return RedirectToAction("Login", "Auth");
        var (items, _) = await api.SendAsync<List<MeetingItem>>(
            HttpMethod.Get,
            "api/meetings/history"
        );
        return View(
            new DashboardViewModel
            {
                Name = HttpContext.Session.GetString("Name")!,
                Meetings = items ?? [],
            }
        );
    }

    [HttpPost("/dashboard/create")]
    public async Task<IActionResult> Create()
    {
        var (m, e) = await api.SendAsync<MeetingItem>(HttpMethod.Post, "api/meetings");
        if (m == null)
        {
            TempData["Error"] = e;
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("Created", "Meeting", new { code = m.RoomCode });
    }
}

public class MeetingController(ApiClientService api) : Controller
{
    [HttpGet("/meeting/created/{code}")]
    public IActionResult Created(string code)
    {
        ViewBag.Code = code;
        ViewBag.Link = Url.Action(nameof(Join), "Meeting", new { roomCode = code }, Request.Scheme);
        return View();
    }

    [HttpGet("/join/{roomCode?}")]
    public IActionResult Join(string? roomCode)
    {
        if (HttpContext.Session.GetString("Token") == null)
            return RedirectToAction("Login", "Auth");
        ViewBag.RoomCode = roomCode;
        ViewBag.Language = HttpContext.Session.GetString("Language") ?? "en";
        return View();
    }

    [HttpPost("/join/{roomCode?}")]
    public async Task<IActionResult> JoinRoom(string? roomCode, string language)
    {
        roomCode = (roomCode ?? Request.Form["roomCode"].ToString()).Trim().ToUpperInvariant();
        var (m, e) = await api.SendAsync<MeetingItem>(
            HttpMethod.Post,
            $"api/meetings/{roomCode}/join",
            new { selectedLanguage = language }
        );
        if (m == null)
        {
            TempData["Error"] = e;
            return RedirectToAction(nameof(Join), new { roomCode });
        }
        return RedirectToAction(nameof(Room), new { roomCode, language });
    }

    [HttpGet("/meeting/{roomCode}")]
    public async Task<IActionResult> Room(string roomCode, string language = "en")
    {
        var (m, _) = await api.SendAsync<MeetingItem>(HttpMethod.Get, $"api/meetings/{roomCode}");
        if (m == null)
            return NotFound();
        return View(
            new MeetingViewModel
            {
                Id = m.Id,
                RoomCode = m.RoomCode,
                Language = language,
                Token = HttpContext.Session.GetString("Token")!,
                DisplayName = HttpContext.Session.GetString("Name")!,
            }
        );
    }

    [HttpPost("/meeting/{roomCode}/leave")]
    public async Task<IActionResult> Leave(string roomCode)
    {
        await api.SendAsync<object>(HttpMethod.Post, $"api/meetings/{roomCode}/leave");
        return RedirectToAction("Index", "Dashboard");
    }
}

public class TranscriptController(ApiClientService api) : Controller
{
    [HttpGet("/transcripts/{meetingId:int}")]
    public async Task<IActionResult> Index(int meetingId)
    {
        var (x, e) = await api.SendAsync<List<TranscriptItem>>(
            HttpMethod.Get,
            $"api/transcripts/meeting/{meetingId}"
        );
        if (x == null)
            return Forbid();
        ViewBag.MeetingId = meetingId;
        return View(x);
    }
}
