import { Link } from 'react-router-dom'

const getField = (source, camelName, pascalName, fallback = '') =>
  source?.[camelName] ?? source?.[pascalName] ?? fallback

const formatWorkFormat = (value) => {
  const labels = {
    Online: 'Онлайн',
    Offline: 'Офлайн',
    Hybrid: 'Гібрид',
  }

  return labels[value] || value || 'Не вказано'
}

const formatProjectType = (value) => {
  const labels = {
    CourseWork: 'Курсова',
    Hackathon: 'Хакатон',
    Startup: 'Стартап',
    0: 'Курсова',
    1: 'Хакатон',
    2: 'Стартап',
  }

  return labels[value] || value || 'Проєкт'
}

function ProjectCard({ project, actions }) {
  const id = getField(project, 'id', 'Id')
  const title = getField(project, 'title', 'Title', 'Без назви')
  const description = getField(project, 'description', 'Description', '')
  const workFormat = getField(project, 'workFormat', 'WorkFormat')
  const projectType = getField(project, 'projectType', 'ProjectType')
  const language = getField(project, 'language', 'Language', 'Не вказано')
  const matchScore = getField(project, 'matchScore', 'MatchScore', null)
  const shouldShowMatchScore = Number(matchScore) > 0

  return (
    <article className="flex h-full flex-col rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg">
      <div className="flex items-start justify-between gap-3">
        <span className="rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">
          {formatProjectType(projectType)}
        </span>
        {shouldShowMatchScore && (
          <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
            {matchScore}% збіг
          </span>
        )}
      </div>

      <h3 className="mt-4 text-lg font-bold text-slate-950">{title}</h3>
      <p className="mt-2 line-clamp-3 flex-1 text-sm leading-6 text-slate-600">
        {description || 'Опис проєкту поки не додано.'}
      </p>

      <div className="mt-5 flex flex-wrap gap-2 text-xs font-medium">
        <span className="rounded-full bg-slate-100 px-3 py-1 text-slate-700">
          {formatWorkFormat(workFormat)}
        </span>
        <span className="rounded-full bg-slate-100 px-3 py-1 text-slate-700">
          {language}
        </span>
      </div>

      <div className="mt-5 flex gap-2">
        <Link
          className="inline-flex flex-1 justify-center rounded-lg bg-slate-950 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
          to={`/projects/${id}`}
        >
          Переглянути
        </Link>
        {actions}
      </div>
    </article>
  )
}

export default ProjectCard
