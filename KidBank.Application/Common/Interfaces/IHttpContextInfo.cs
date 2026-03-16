namespace KidBank.Application.Common.Interfaces;

public interface IHttpContextInfo
{
    string? RequestPath { get; }
    string? RequestMethod { get; }
    string? RemoteIpAddress { get; }
}
