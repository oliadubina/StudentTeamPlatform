# Student Team Platform - System Architecture & AI Guidelines

## Project Context
This is a "Student Team Matching Platform" designed for the academic context (universities). It helps students form teams for hackathons, courseworks, and startups. It is NOT a commercial job board. The core feature is a matching algorithm based on skills, roles, preferred format, and language.

## Architecture (Full-Stack)
- **Backend:** ASP.NET Core Web API (C#). Located in `StudentTeamPlatform.Api`.
- **Frontend:** React 19 + Vite (SPA). Located in the `client/` folder.
- **Database:** MSSQL via Entity Framework Core (Code-First).
- **Communication:** REST API (HTTPS) and WebSockets (SignalR for group chat).

## Global AI Rules
1. **Language:** The user interface (UI) text MUST be in Ukrainian. Code comments can be in English.
2. **No Mocking:** Do NOT generate hardcoded mock data. Always use Axios to fetch data from the REST API.
3. **Authentication:** JWT-based. Tokens must be passed in the `Authorization: Bearer <token>` header.
4. **Scope Limitations:** - There is NO Admin role in this MVP.
   - There is NO Database-backed Notification system (do not create notification endpoints). We use UI toasts only.
   - Hard deletion of profiles is replaced by Soft Delete or omitted.

## Database Entities (Context for Frontend)
- `User` (Id, FullName, Email, PasswordHash, Skills, PreferredFormat, PreferredLanguage)
- `Project` (Id, Title, Description, AuthorId, ProjectState, MaxContributors)
- `ProjectRole` (Id, ProjectId, Name, SlotsCount)
- `JoinRequest` (Id, ProjectId, StudentId, ProjectRoleId, Status [Pending, Accepted, Rejected])
- `ChatMessage` (For SignalR chat history)