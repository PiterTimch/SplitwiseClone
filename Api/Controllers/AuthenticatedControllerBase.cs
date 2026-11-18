namespace SplitwiseClone.Api.Controllers;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Application.Exceptions;

public abstract class AuthenticatedControllerBase : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(value, out var userId) && userId > 0
                ? userId
                : throw new UserUnauthorizedException("The authenticated user identifier is invalid.");
        }
    }
}
