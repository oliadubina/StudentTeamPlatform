import { Link as RouterLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/authContext'

function Navbar() {
  const { isAuthenticated, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <header className="border-b border-slate-200 bg-white">
      <nav className="mx-auto flex max-w-6xl flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:justify-between">
        <RouterLink className="text-lg font-bold text-slate-950" to="/">
          Student Team Platform
        </RouterLink>

        <div className="flex flex-wrap items-center gap-2">
          <RouterLink
            className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
            to="/"
          >
            Головна
          </RouterLink>

          {isAuthenticated ? (
            <>
              <RouterLink
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
                to="/projects"
              >
                Проєкти
              </RouterLink>
              <RouterLink
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
                to="/my-projects"
              >
                Мої проєкти
              </RouterLink>
              <RouterLink
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
                to="/my-requests"
              >
                Мої заявки
              </RouterLink>
              <RouterLink
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
                to="/chats"
              >
                Чати
              </RouterLink>
              <RouterLink
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
                to="/profile"
              >
                Мій профіль
              </RouterLink>
              <button
                className="rounded-lg bg-slate-950 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800"
                onClick={handleLogout}
                type="button"
              >
                Вийти
              </button>
            </>
          ) : (
            <>
              <RouterLink
                className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100 hover:text-slate-950"
                to="/login"
              >
                Увійти
              </RouterLink>
              <RouterLink
                className="rounded-lg bg-blue-600 px-3 py-2 text-sm font-medium text-white hover:bg-blue-700"
                to="/register"
              >
                Зареєструватися
              </RouterLink>
            </>
          )}
        </div>
      </nav>
    </header>
  )
}

export default Navbar
