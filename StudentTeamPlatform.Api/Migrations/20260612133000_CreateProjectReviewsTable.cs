using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTeamPlatform.Api.Migrations
{
    [Migration("20260612133000_CreateProjectReviewsTable")]
    public partial class CreateProjectReviewsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ProjectReviews]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProjectReviews] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [ReviewerId] int NOT NULL,
        [RevieweeId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProjectReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectReviews_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProjectReviews_Users_RevieweeId] FOREIGN KEY ([RevieweeId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProjectReviews_Users_ReviewerId] FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_ProjectReviews_ProjectId_ReviewerId_RevieweeId' AND [object_id] = OBJECT_ID(N'[dbo].[ProjectReviews]'))
BEGIN
    CREATE UNIQUE INDEX [IX_ProjectReviews_ProjectId_ReviewerId_RevieweeId]
    ON [dbo].[ProjectReviews] ([ProjectId], [ReviewerId], [RevieweeId]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_ProjectReviews_RevieweeId' AND [object_id] = OBJECT_ID(N'[dbo].[ProjectReviews]'))
BEGIN
    CREATE INDEX [IX_ProjectReviews_RevieweeId] ON [dbo].[ProjectReviews] ([RevieweeId]);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_ProjectReviews_ReviewerId' AND [object_id] = OBJECT_ID(N'[dbo].[ProjectReviews]'))
BEGIN
    CREATE INDEX [IX_ProjectReviews_ReviewerId] ON [dbo].[ProjectReviews] ([ReviewerId]);
END;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[ProjectReviews]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[ProjectReviews];
END;
");
        }
    }
}
