using KidBank.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KidBank.Infrastructure.Services;

public class HttpContextInfoService : IHttpContextInfo
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextInfoService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? RequestPath => _accessor.HttpContext?.Request.Path.Value;
    public string? RequestMethod => _accessor.HttpContext?.Request.Method;
    public string? RemoteIpAddress => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
