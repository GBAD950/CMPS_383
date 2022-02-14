using System;
using System.Net.Http;
using System.Threading.Tasks;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.AuthenticationController;

internal static class HttpClientExtensions
{
    public const string DefaultUserPassword = "Password123!";

    public static async Task<bool> LogoutAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsync("/api/authentication/logout", null);
            responseMessage.AssertLogoutFunctions();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task<UserDto?> LoginAsAdminAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
            {
                UserName = "galkadi",
                Password = DefaultUserPassword
            });
            return await responseMessage.AssertLoginFunctions();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<UserDto?> LoginAsBobAsync(this HttpClient webClient)
    {
        try
        {
            var responseMessage = await webClient.PostAsJsonAsync("/api/authentication/login", new LoginDto
            {
                UserName = "bob",
                Password = DefaultUserPassword
            });
            return await responseMessage.AssertLoginFunctions();
        }
        catch (Exception)
        {
            return null;
        }
    }
}