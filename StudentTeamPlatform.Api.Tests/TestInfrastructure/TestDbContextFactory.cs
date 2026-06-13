using Microsoft.EntityFrameworkCore;
using StudentTeamPlatform.Api.Data;

namespace StudentTeamPlatform.Api.Tests.TestInfrastructure;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
