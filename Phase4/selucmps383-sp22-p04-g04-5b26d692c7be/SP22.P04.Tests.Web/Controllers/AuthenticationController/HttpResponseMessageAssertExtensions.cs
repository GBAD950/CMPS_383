using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P04.Tests.Web.Dtos;
using SP22.P04.Tests.Web.Helpers;

namespace SP22.P04.Tests.Web.Controllers.AuthenticationController;

internal static class HttpResponseMessageAssertExtensions
{
    public static async Task<UserDto> AssertLoginFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling POST /api/authentication/login when a valid username / password is given");
        httpResponse.Headers.Should().ContainKey("Set-Cookie", "we expect that a login operation will use Set-Cookie to log the user in");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<UserDto>();
        resultDto.Should().NotBeNull("we expect a UserDto as the result of logging in");
        Assert.IsNotNull(resultDto);
        resultDto.Id.Should().BeGreaterThan(0, "we should have a valid user Id returned after logging in");
        resultDto.UserName.Should().NotBeNullOrEmpty("we should have a valid user name returned after logging in");

        return resultDto;
    }

    public static void AssertLogoutFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling POST /api/authentication/logout while logged in");
        httpResponse.Headers.Should().ContainKey("Set-Cookie", "we expect that a logout operation will use Set-Cookie to log the user out");
    }
}