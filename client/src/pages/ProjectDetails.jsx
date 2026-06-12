import { Check, Flag, Send, Star, X } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { toast } from 'react-toastify'
import api from '../api/axiosConfig'

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

const formatProjectState = (value) => {
  const labels = {
    SearchTeam: 'Пошук команди',
    InProcess: 'У роботі',
    Done: 'Завершено',
    Archieved: 'Архівовано',
  }

  return labels[value] || value || 'Не вказано'
}

const normalizeRole = (role, index) => ({
  id: getField(role, 'id', 'Id', 0),
  name: getField(role, 'name', 'Name', `Роль ${index + 1}`),
  slotsCount: getField(role, 'slotsCount', 'SlotsCount', 0),
})

const normalizeRequest = (request) => ({
  id: getField(request, 'id', 'Id'),
  studentId: getField(request, 'studentId', 'StudentId'),
  studentName: getField(request, 'studentName', 'StudentName', 'Студент'),
  projectRoleName: getField(request, 'projectRoleName', 'ProjectRoleName', 'Роль не вказана'),
  status: getField(request, 'status', 'Status', 'Pending'),
})

const normalizeContributor = (contributor) => ({
  id: getField(contributor, 'id', 'Id'),
  fullName: getField(contributor, 'fullName', 'FullName', 'Учасник'),
  email: getField(contributor, 'email', 'Email', ''),
})

const normalizeProject = (data) => ({
  id: getField(data, 'id', 'Id'),
  title: getField(data, 'title', 'Title', 'Без назви'),
  description: getField(data, 'description', 'Description'),
  author: getField(data, 'authorName', 'AuthorName', {}),
  authorName: getField(getField(data, 'authorName', 'AuthorName', {}), 'fullName', 'FullName'),
  isAuthor: getField(data, 'isAuthor', 'IsAuthor', false),
  isContributor: getField(data, 'isContributor', 'IsContributor', false),
  language: getField(data, 'language', 'Language', 'Не вказано'),
  workFormat: getField(data, 'workFormat', 'WorkFormat'),
  projectState: getField(data, 'projectState', 'ProjectState'),
  projectRoles: (getField(data, 'projectRoles', 'ProjectRoles', []) || []).map(normalizeRole),
  technologies: (getField(data, 'technology', 'Technology', []) || []).map((tech) =>
    getField(tech, 'name', 'Name'),
  ),
  contributors: (getField(data, 'contributors', 'Contributors', []) || []).map(normalizeContributor),
  pendingRequests: (getField(data, 'pendingRequests', 'PendingRequests', []) || []).map(
    normalizeRequest,
  ),
})

const normalizePendingReview = (review) => ({
  userId: getField(review, 'userId', 'UserId'),
  fullName: getField(review, 'fullName', 'FullName', 'Учасник'),
  email: getField(review, 'email', 'Email', ''),
})

function ProjectDetailsSkeleton() {
  return (
    <div className="rounded-xl bg-white p-8 shadow-lg">
      <div className="h-8 w-2/3 animate-pulse rounded bg-slate-200" />
      <div className="mt-4 h-4 animate-pulse rounded bg-slate-100" />
      <div className="mt-2 h-4 w-5/6 animate-pulse rounded bg-slate-100" />
      <div className="mt-8 grid gap-4 md:grid-cols-3">
        <div className="h-20 animate-pulse rounded-lg bg-slate-100" />
        <div className="h-20 animate-pulse rounded-lg bg-slate-100" />
        <div className="h-20 animate-pulse rounded-lg bg-slate-100" />
      </div>
    </div>
  )
}

function ProjectDetails() {
  const { projectId } = useParams()
  const [project, setProject] = useState(null)
  const [selectedRoleId, setSelectedRoleId] = useState('')
  const [pendingReviews, setPendingReviews] = useState([])
  const [reviewForms, setReviewForms] = useState({})
  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingReviews, setIsLoadingReviews] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const availableRoles = useMemo(
    () => project?.projectRoles.filter((role) => role.slotsCount > 0 && role.id) || [],
    [project],
  )

  const canApply = project && !project.isAuthor && !project.isContributor && project.projectState === 'SearchTeam'
  const canComplete = project?.isAuthor && project.projectState !== 'Done'
  const canReview = project && (project.isAuthor || project.isContributor) && project.projectState === 'Done'

  const fetchPendingReviews = useCallback(async () => {
    setIsLoadingReviews(true)

    try {
      const response = await api.get(`/api/review/project/${projectId}/pending`)
      const normalizedReviews = (response.data || []).map(normalizePendingReview)
      setPendingReviews(normalizedReviews)
      setReviewForms((current) => {
        const nextForms = { ...current }
        normalizedReviews.forEach((review) => {
          if (!nextForms[review.userId]) {
            nextForms[review.userId] = { rating: 5, comment: '' }
          }
        })
        return nextForms
      })
    } catch (error) {
      toast.error(error.response?.data?.message || 'Не вдалося завантажити список відгуків')
    } finally {
      setIsLoadingReviews(false)
    }
  }, [projectId])

  const fetchProject = useCallback(async () => {
    try {
      const response = await api.get(`/api/project/${projectId}`)
      const normalizedProject = normalizeProject(response.data)
      setProject(normalizedProject)
      setSelectedRoleId(normalizedProject.projectRoles.find((role) => role.slotsCount > 0)?.id || '')
    } catch (error) {
      toast.error(error.response?.data?.message || 'Не вдалося завантажити проєкт')
    } finally {
      setIsLoading(false)
    }
  }, [projectId])

  useEffect(() => {
    const timeoutId = setTimeout(fetchProject, 0)

    return () => clearTimeout(timeoutId)
  }, [fetchProject])

  useEffect(() => {
    if (!canReview) {
      return
    }

    const timeoutId = setTimeout(fetchPendingReviews, 0)

    return () => clearTimeout(timeoutId)
  }, [canReview, fetchPendingReviews])

  const handleApply = async () => {
    if (!selectedRoleId) {
      toast.error('Оберіть роль для заявки')
      return
    }

    setIsSubmitting(true)

    try {
      await api.post(`/api/joinrequest/apply/${projectId}/role/${selectedRoleId}`)
      toast.success('Заявку успішно надіслано!')
      await fetchProject()
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося подати заявку')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleRespond = async (requestId, status) => {
    setIsSubmitting(true)

    try {
      await api.put(`/api/joinrequest/${requestId}/respond?status=${status}`)
      toast.success(status === 'Accepted' ? 'Заявку прийнято' : 'Заявку відхилено')
      await fetchProject()
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося обробити заявку')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCompleteProject = async () => {
    setIsSubmitting(true)

    try {
      await api.put(`/api/project/${projectId}/complete`)
      toast.success('Проєкт завершено. Команда може залишити відгуки.')
      await fetchProject()
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося завершити проєкт')
    } finally {
      setIsSubmitting(false)
    }
  }

  const updateReviewForm = (userId, field, value) => {
    setReviewForms((current) => ({
      ...current,
      [userId]: {
        rating: 5,
        comment: '',
        ...current[userId],
        [field]: value,
      },
    }))
  }

  const handleSubmitReview = async (userId) => {
    const form = reviewForms[userId] || { rating: 5, comment: '' }

    setIsSubmitting(true)

    try {
      await api.post('/api/review', {
        projectId: Number(projectId),
        revieweeId: userId,
        rating: Number(form.rating),
        comment: form.comment,
      })
      toast.success('Відгук збережено')
      await fetchPendingReviews()
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося зберегти відгук')
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <ProjectDetailsSkeleton />
  }

  if (!project) {
    return (
      <div className="rounded-xl bg-white p-8 text-center shadow-lg">
        <p className="text-slate-600">Проєкт не знайдено.</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <section className="rounded-xl bg-white p-6 shadow-lg md:p-8">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-slate-950">
              {project.title}
            </h1>
            <p className="mt-2 text-sm text-slate-500">
              Автор: {project.authorName || 'Не вказано'}
            </p>
          </div>

          {canComplete && (
            <button
              className="inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-2.5 font-semibold text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:bg-emerald-300"
              disabled={isSubmitting}
              onClick={handleCompleteProject}
              type="button"
            >
              <Flag size={16} />
              Завершити проєкт
            </button>
          )}

          {canApply && (
            <div className="w-full rounded-xl border border-slate-200 bg-slate-50 p-4 lg:w-80">
              <label className="text-sm font-medium text-slate-700" htmlFor="role">
                Роль для заявки
              </label>
              <select
                className="mt-2 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                id="role"
                onChange={(event) => setSelectedRoleId(event.target.value)}
                value={selectedRoleId}
              >
                {availableRoles.length === 0 && <option value="">Немає доступних ролей</option>}
                {availableRoles.map((role) => (
                  <option key={role.id} value={role.id}>
                    {role.name} - місць: {role.slotsCount}
                  </option>
                ))}
              </select>
              <button
                className="mt-3 inline-flex w-full items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
                disabled={isSubmitting || !selectedRoleId}
                onClick={handleApply}
                type="button"
              >
                <Send size={16} />
                Подати заявку
              </button>
            </div>
          )}
        </div>

        <p className="mt-6 leading-7 text-slate-700">
          {project.description || 'Опис проєкту поки не додано.'}
        </p>

        <div className="mt-8 grid gap-4 md:grid-cols-4">
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Формат</p>
            <p className="mt-1 font-medium text-slate-950">{formatWorkFormat(project.workFormat)}</p>
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Мова</p>
            <p className="mt-1 font-medium text-slate-950">{project.language}</p>
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Стан</p>
            <p className="mt-1 font-medium text-slate-950">{formatProjectState(project.projectState)}</p>
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Ваша роль</p>
            <p className="mt-1 font-medium text-slate-950">
              {project.isAuthor ? 'Автор' : project.isContributor ? 'Учасник' : 'Гість'}
            </p>
          </div>
        </div>

        <div className="mt-6">
          <h2 className="font-semibold text-slate-950">Технології</h2>
          <div className="mt-3 flex flex-wrap gap-2">
            {project.technologies.length > 0 ? (
              project.technologies.map((technology) => (
                <span
                  className="rounded-full bg-blue-50 px-3 py-1 text-sm font-medium text-blue-700"
                  key={technology}
                >
                  {technology}
                </span>
              ))
            ) : (
              <p className="text-sm text-slate-500">Технології не вказані.</p>
            )}
          </div>
        </div>

        <div className="mt-6">
          <h2 className="font-semibold text-slate-950">Учасники</h2>
          <div className="mt-3 flex flex-wrap gap-2">
            {project.contributors.length > 0 ? (
              project.contributors.map((contributor) => (
                <Link
                  className="rounded-full bg-slate-100 px-3 py-1 text-sm font-medium text-slate-700 hover:bg-blue-50 hover:text-blue-700"
                  key={contributor.id}
                  to={`/users/${contributor.id}`}
                >
                  {contributor.fullName}
                </Link>
              ))
            ) : (
              <p className="text-sm text-slate-500">У команді ще немає учасників.</p>
            )}
          </div>
        </div>

        <div className="mt-6">
          <h2 className="font-semibold text-slate-950">Ролі</h2>
          <div className="mt-3 grid gap-3 md:grid-cols-2">
            {project.projectRoles.map((role) => (
              <div className="rounded-lg border border-slate-200 p-4" key={role.id || role.name}>
                <p className="font-medium text-slate-950">{role.name}</p>
                <p className="mt-1 text-sm text-slate-500">Вільних місць: {role.slotsCount}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {canReview && (
        <section className="rounded-xl bg-white p-6 shadow-lg md:p-8">
          <div className="flex items-center gap-2">
            <Star className="text-amber-500" size={22} />
            <h2 className="text-xl font-bold text-slate-950">Відгуки про команду</h2>
          </div>
          <p className="mt-2 text-sm text-slate-500">
            Оцініть кожного учасника проєкту за шкалою від 1 до 5 і залиште короткий коментар.
          </p>

          <div className="mt-5 space-y-4">
            {isLoadingReviews ? (
              <>
                <div className="h-32 animate-pulse rounded-lg bg-slate-100" />
                <div className="h-32 animate-pulse rounded-lg bg-slate-100" />
              </>
            ) : pendingReviews.length > 0 ? (
              pendingReviews.map((reviewUser) => {
                const form = reviewForms[reviewUser.userId] || { rating: 5, comment: '' }

                return (
                  <div className="rounded-lg border border-slate-200 p-4" key={reviewUser.userId}>
                    <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                      <div>
                        <p className="font-semibold text-slate-950">{reviewUser.fullName}</p>
                        <p className="text-sm text-slate-500">{reviewUser.email}</p>
                      </div>
                      <select
                        className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200 md:w-32"
                        onChange={(event) => updateReviewForm(reviewUser.userId, 'rating', event.target.value)}
                        value={form.rating}
                      >
                        {[5, 4, 3, 2, 1].map((rating) => (
                          <option key={rating} value={rating}>
                            {rating} / 5
                          </option>
                        ))}
                      </select>
                    </div>

                    <textarea
                      className="mt-3 min-h-24 w-full rounded-lg border border-slate-300 px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                      maxLength={1000}
                      onChange={(event) => updateReviewForm(reviewUser.userId, 'comment', event.target.value)}
                      placeholder="Напишіть короткий відгук про співпрацю..."
                      value={form.comment}
                    />

                    <button
                      className="mt-3 inline-flex items-center justify-center gap-2 rounded-lg bg-slate-950 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:bg-slate-400"
                      disabled={isSubmitting}
                      onClick={() => handleSubmitReview(reviewUser.userId)}
                      type="button"
                    >
                      <Send size={16} />
                      Зберегти відгук
                    </button>
                  </div>
                )
              })
            ) : (
              <p className="rounded-lg border border-dashed border-slate-300 p-6 text-center text-slate-500">
                Усі потрібні відгуки вже залишено.
              </p>
            )}
          </div>
        </section>
      )}

      {project.isAuthor && (
        <section className="rounded-xl bg-white p-6 shadow-lg md:p-8">
          <h2 className="text-xl font-bold text-slate-950">Вхідні заявки</h2>
          <div className="mt-5 space-y-3">
            {project.pendingRequests.length > 0 ? (
              project.pendingRequests.map((request) => (
                <div
                  className="flex flex-col gap-3 rounded-lg border border-slate-200 p-4 sm:flex-row sm:items-center sm:justify-between"
                  key={request.id}
                >
                  <div>
                    {request.studentId ? (
                      <Link
                        className="font-medium text-slate-950 hover:text-blue-700"
                        to={`/users/${request.studentId}`}
                      >
                        {request.studentName}
                      </Link>
                    ) : (
                      <p className="font-medium text-slate-950">{request.studentName}</p>
                    )}
                    <p className="text-sm text-slate-500">
                      Роль: {request.projectRoleName} · Статус: {request.status}
                    </p>
                  </div>
                  {request.status === 'Pending' && project.projectState !== 'Done' && (
                    <div className="flex gap-2">
                      <button
                        className="inline-flex items-center gap-2 rounded-lg bg-emerald-600 px-3 py-2 text-sm font-medium text-white hover:bg-emerald-700 disabled:bg-emerald-300"
                        disabled={isSubmitting}
                        onClick={() => handleRespond(request.id, 'Accepted')}
                        type="button"
                      >
                        <Check size={16} />
                        Прийняти
                      </button>
                      <button
                        className="inline-flex items-center gap-2 rounded-lg bg-red-600 px-3 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:bg-red-300"
                        disabled={isSubmitting}
                        onClick={() => handleRespond(request.id, 'Rejected')}
                        type="button"
                      >
                        <X size={16} />
                        Відхилити
                      </button>
                    </div>
                  )}
                </div>
              ))
            ) : (
              <p className="rounded-lg border border-dashed border-slate-300 p-6 text-center text-slate-500">
                Заявок поки немає.
              </p>
            )}
          </div>
        </section>
      )}
    </div>
  )
}

export default ProjectDetails
