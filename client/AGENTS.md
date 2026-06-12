# Frontend (React) AI Developer Guidelines

## Tech Stack & Libraries
- **Framework:** React 19 (Vite)
- **Routing:** `react-router-dom` (v6)
- **HTTP Client:** `axios`
- **UI & Styling:** Tailwind CSS. Do NOT use Material UI (MUI) or any pre-built UI frameworks. Use pure HTML elements (`div`, `button`, `input`) styled with Tailwind utility classes. For icons, use `lucide-react`. Make the design look modern, clean, and minimalist (similar to shadcn/ui style).
- **Form Handling:** `react-hook-form` + `@hookform/resolvers/zod` + `zod` for validation.
- **UI Notifications:** `react-toastify` (used for success/error messages).
- **Real-time Chat:** `@microsoft/signalr`

## Project Structure (Enforce this)
- `/src/api` - Axios instances and interceptors (attach JWT here).
- `/src/pages` - Page components (e.g., `Dashboard.jsx`, `Profile.jsx`, `ProjectDetails.jsx`, `Login.jsx`).
- `/src/components` - Reusable UI (e.g., `Navbar.jsx`, `ProjectCard.jsx`, `ChatWindow.jsx`).
- `/src/contexts` - React Context (e.g., `AuthContext.jsx` for global user state).
- `/src/hooks` - Custom hooks (e.g., `useSignalR.js`).

## Coding Rules for AI
1. **Component Style:** Use Functional Components with React Hooks ONLY. No Class Components.
2. **Material UI Usage:** Use MUI components (`Box`, `Typography`, `Button`, `TextField`, `Grid`, `Card`). Do not write raw CSS files unless strictly necessary. Ensure responsive design.
3. **Forms:** Use `react-hook-form` coupled with `zod` for all inputs (Login, Register, Create Project). Show validation errors below fields in Ukrainian.
4. **API Error Handling:** Wrap API calls in `try/catch`. On error, trigger a `toast.error("Помилка: [message]")`. On success for mutations, trigger `toast.success()`.
5. **Auth State:** Rely on `AuthContext` to get `currentUser` and `isAuthenticated`.

## Business Logic (Strict SRS Compliance)
1. **Project Details Page Buttons:**
   When fetching a project by ID, the API returns `IsAuthor` and `IsContributor` boolean flags.
   - IF `IsAuthor === true`: Render "Редагувати" (Edit), "Видалити" (Delete), and a section for "Вхідні заявки" (Incoming Join Requests).
   - IF `IsContributor === true`: Render "Груповий чат" (Group Chat) and "Покинути команду" (Leave Team).
   - ELSE: Render "Подати заявку" (Apply) dropdown/modal to select a specific `ProjectRole`.
2. **Search Logic:** The search uses `POST /api/project/search` with a `ProjectFilterDTO` body. Do NOT use a GET request for search.
3. **Join Requests:** Students apply for a *specific role* (`roleId`), not just the project.
4. **Loading States:** Use MUI `<Skeleton />` loaders instead of circular spinners when fetching initial page data (like Dashboard or Project lists) for better UX.