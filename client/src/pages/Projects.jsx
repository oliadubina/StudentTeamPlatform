import { Search, X } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { toast } from 'react-toastify'
import api from '../api/axiosConfig'
import ProjectCard from '../components/ProjectCard'

const TABS = {
  all: 'all',
  recommended: 'recommended',
}

const PROJECT_TYPE_OPTIONS = [
  { value: 'CourseWork', label: 'Курсова' },
  { value: 'Hackathon', label: 'Хакатон' },
  { value: 'Startup', label: 'Стартап' },
]

const WORK_FORMAT_OPTIONS = [
  { value: 'Online', label: 'Онлайн' },
  { value: 'Offline', label: 'Офлайн' },
  { value: 'Hybrid', label: 'Гібрид' },
]

const LANGUAGE_OPTIONS = [
  { value: 'Ukrainian', label: 'Українська' },
  { value: 'English', label: 'Англійська' },
  { value: 'Polish', label: 'Польська' },
  { value: 'German', label: 'Німецька' },
]

function ProjectCardSkeleton() {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex justify-between">
        <div className="h-6 w-24 animate-pulse rounded-full bg-slate-200" />
        <div className="h-6 w-20 animate-pulse rounded-full bg-slate-200" />
      </div>
      <div className="mt-5 h-6 w-3/4 animate-pulse rounded bg-slate-200" />
      <div className="mt-4 space-y-2">
        <div className="h-4 animate-pulse rounded bg-slate-100" />
        <div className="h-4 animate-pulse rounded bg-slate-100" />
        <div className="h-4 w-2/3 animate-pulse rounded bg-slate-100" />
      </div>
      <div className="mt-6 h-10 animate-pulse rounded-lg bg-slate-200" />
    </div>
  )
}

function Projects() {
  const [activeTab, setActiveTab] = useState(TABS.all)
  const [projects, setProjects] = useState([])
  const [filters, setFilters] = useState({
    searchKeyword: '',
    projectType: '',
    workFormat: '',
    language: '',
    role: '',
  })
  const [isLoading, setIsLoading] = useState(true)

  const searchPayload = useMemo(
    () => ({
      searchKeyword: filters.searchKeyword.trim(),
      projectType: filters.projectType || null,
      workFormat: filters.workFormat || null,
      language: filters.language,
      role: filters.role.trim(),
      technologies: [],
    }),
    [filters],
  )

  useEffect(() => {
    const timeoutId = setTimeout(async () => {
      setIsLoading(true)

      try {
        const response =
          activeTab === TABS.recommended
            ? await api.get('/api/project/recommended-projects')
            : await api.post('/api/project/search', searchPayload)

        setProjects(response.data || [])
      } catch (error) {
        toast.error(error.response?.data?.message || 'Не вдалося завантажити проєкти')
      } finally {
        setIsLoading(false)
      }
    }, activeTab === TABS.all ? 300 : 0)

    return () => clearTimeout(timeoutId)
  }, [activeTab, searchPayload])

  const updateFilter = (name, value) => {
    setFilters((current) => ({ ...current, [name]: value }))
  }

  const resetFilters = () => {
    setFilters({
      searchKeyword: '',
      projectType: '',
      workFormat: '',
      language: '',
      role: '',
    })
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-slate-950">
          Пошук проєктів
        </h1>
        <p className="mt-2 text-slate-600">
          Знайдіть команду або перегляньте рекомендації на основі вашого профілю.
        </p>
      </div>

      <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div className="relative">
          <Search
            className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-slate-400"
            size={20}
          />
          <input
            className="w-full rounded-lg border border-slate-300 py-3 pl-10 pr-4 text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 disabled:bg-slate-100"
            disabled={activeTab === TABS.recommended}
            onChange={(event) => updateFilter('searchKeyword', event.target.value)}
            placeholder="Введіть назву або опис проєкту"
            type="search"
            value={filters.searchKeyword}
          />
        </div>

        <div className="mt-4 inline-flex rounded-lg bg-slate-100 p-1">
          <button
            className={`rounded-md px-4 py-2 text-sm font-medium transition ${
              activeTab === TABS.all
                ? 'bg-white text-slate-950 shadow-sm'
                : 'text-slate-600 hover:text-slate-950'
            }`}
            onClick={() => setActiveTab(TABS.all)}
            type="button"
          >
            Усі проєкти
          </button>
          <button
            className={`rounded-md px-4 py-2 text-sm font-medium transition ${
              activeTab === TABS.recommended
                ? 'bg-white text-slate-950 shadow-sm'
                : 'text-slate-600 hover:text-slate-950'
            }`}
            onClick={() => setActiveTab(TABS.recommended)}
            type="button"
          >
            Рекомендовані для мене
          </button>
        </div>

        {activeTab === TABS.all && (
          <div className="mt-5 grid gap-3 md:grid-cols-5">
            <select
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
              onChange={(event) => updateFilter('projectType', event.target.value)}
              value={filters.projectType}
            >
              <option value="">Тип проєкту</option>
              {PROJECT_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
              onChange={(event) => updateFilter('workFormat', event.target.value)}
              value={filters.workFormat}
            >
              <option value="">Формат</option>
              {WORK_FORMAT_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <select
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
              onChange={(event) => updateFilter('language', event.target.value)}
              value={filters.language}
            >
              <option value="">Мова</option>
              {LANGUAGE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <input
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
              onChange={(event) => updateFilter('role', event.target.value)}
              placeholder="Роль"
              type="text"
              value={filters.role}
            />

            <button
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
              onClick={resetFilters}
              type="button"
            >
              <X size={16} />
              Очистити
            </button>
          </div>
        )}
      </div>

      <div className="mt-8">
        {isLoading ? (
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            <ProjectCardSkeleton />
            <ProjectCardSkeleton />
            <ProjectCardSkeleton />
          </div>
        ) : projects.length > 0 ? (
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
            {projects.map((project, index) => (
              <ProjectCard
                key={project.id ?? project.Id ?? `${project.title}-${index}`}
                project={project}
              />
            ))}
          </div>
        ) : (
          <div className="rounded-xl border border-dashed border-slate-300 bg-white p-10 text-center">
            <p className="font-medium text-slate-700">
              За вашим запитом нічого не знайдено
            </p>
          </div>
        )}
      </div>
    </div>
  )
}

export default Projects
