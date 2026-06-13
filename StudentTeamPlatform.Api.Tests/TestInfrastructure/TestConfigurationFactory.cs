using Microsoft.Extensions.Configuration;

namespace StudentTeamPlatform.Api.Tests.TestInfrastructure;

public static class TestConfigurationFactory
{
    public static IConfiguration Create()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "student-team-platform-test-key-with-enough-length",
                ["Jwt:Issuer"] = "StudentTeamPlatform.Tests",
                ["Jwt:Audience"] = "StudentTeamPlatform.Tests"
            })
            .Build();
    }
}
