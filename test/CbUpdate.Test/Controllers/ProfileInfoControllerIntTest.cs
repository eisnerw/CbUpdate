using FluentAssertions;
using CbUpdate.Test.Setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CbUpdate.Test.Controllers;

public class ProfileInfoControllerIntTest
{
    private readonly AppWebApplicationFactory<TestStartup> _factory;

    public ProfileInfoControllerIntTest()
    {
        _factory = new AppWebApplicationFactory<TestStartup>();
    }

    [Fact]
    public async Task TestGetProfileInfos()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/management/info");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = JToken.Parse(await response.Content.ReadAsStringAsync());
        json.SelectToken("$.display-ribbon-on-profiles").Value<string>().Should().Be("dev");
        json.SelectToken("$.activeProfiles").ToObject<IEnumerable<string>>().Should().Contain(new[] { "api-docs", "prod" });
    }
}
