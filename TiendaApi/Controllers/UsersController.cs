using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Features.Users.Requests;
using TiendaApi.Application.Interfaces.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService _userService) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    //public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    //{
    //    var result = await _userService.CreateAsync(request);

    //    return CreatedAtAction(
    //        nameof(GetById),
    //        new { id = result.Id },
    //        result);
    //}

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var requestingUserId = GetUserIdFromClaims();
        await _userService.ToggleActiveAsync(id, requestingUserId);
        return NoContent();
    }
}