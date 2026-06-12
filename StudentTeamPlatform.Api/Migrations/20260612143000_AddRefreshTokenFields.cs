using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTeamPlatform.Api.Migrations
{
    [Migration("20260612143000_AddRefreshTokenFields")]
    public partial class AddRefreshTokenFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[Users]', N'RefreshToken') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users] ADD [RefreshToken] nvarchar(max) NULL;
END;

IF COL_LENGTH(N'[dbo].[Users]', N'RefreshTokenExpiresAt') IS NULL
BEGIN
    ALTER TABLE [dbo].[Users] ADD [RefreshTokenExpiresAt] datetime2 NULL;
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[dbo].[Users]', N'RefreshTokenExpiresAt') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Users] DROP COLUMN [RefreshTokenExpiresAt];
END;

IF COL_LENGTH(N'[dbo].[Users]', N'RefreshToken') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Users] DROP COLUMN [RefreshToken];
END;
");
        }
    }
}
