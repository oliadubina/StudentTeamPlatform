import { Star } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
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

const normalizeReview = (review) => ({
  id: getField(review, 'id', 'Id'),
  projectId: getField(review, 'projectId', 'ProjectId'),
  projectTitle: getField(review, 'projectTitle', 'ProjectTitle', 'Проєкт'),
  reviewerId: getField(review, 'reviewerId', 'ReviewerId'),
  reviewerName: getField(review, 'reviewerName', 'ReviewerName', 'Користувач'),
  rating: getField(review, 'rating', 'Rating', 0),
  comment: getField(review, 'comment', 'Comment', ''),
  createdAt: getField(review, 'createdAt', 'CreatedAt'),
})

const normalizeProfile = (data) => ({
  fullName: getField(data, 'fullName', 'FullName', 'Студент'),
  email: getField(data, 'emailAddress', 'EmailAddress', getField(data, 'email', 'Email')),
  university: getField(data, 'university', 'University'),
  speciality: getField(data, 'speciality', 'Speciality'),
  course: getField(data, 'course', 'Course', 0),
  workFormat: getField(data, 'workFormat', 'WorkFormat'),
  preferredLanguage: getField(data, 'preferredLanguage', 'PreferredLanguage'),
  averageRating: getField(data, 'averageRating', 'AverageRating', 0),
  reviewsCount: getField(data, 'reviewsCount', 'ReviewsCount', 0),
  reviews: (getField(data, 'reviews', 'Reviews', []) || []).map(normalizeReview),
  skills: (getField(data, 'skills', 'Skills', []) || []).map((skill) => ({
    id: getField(skill, 'id', 'Id', `${getField(skill, 'name', 'Name')}-${Date.now()}`),
    name: getField(skill, 'name', 'Name'),
    category: getField(skill, 'category', 'Category'),
  })),
})

const getInitials = (fullName) =>
  fullName
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase() || 'ST'

function RatingStars({ rating }) {
  return (
    <div className="flex items-center gap-1 text-amber-500">
      {[1, 2, 3, 4, 5].map((value) => (
        <Star
          className={value <= Math.round(Number(rating)) ? 'fill-current' : ''}
          key={value}
          size={16}
        />
      ))}
    </div>
  )
}

function ExternalProfile() {
  const { userId } = useParams()
  const [profile, setProfile] = useState(null)
  const [isLoading, setIsLoading] = useState(true)

  const initials = useMemo(() => getInitials(profile?.fullName || ''), [profile])

  const fetchProfile = useCallback(async () => {
    setIsLoading(true)

    try {
      const response = await api.get(`/api/profile/user/${userId}`)
      setProfile(normalizeProfile(response.data))
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося завантажити профіль')
    } finally {
      setIsLoading(false)
    }
  }, [userId])

  useEffect(() => {
    const timeoutId = setTimeout(fetchProfile, 0)

    return () => clearTimeout(timeoutId)
  }, [fetchProfile])

  if (isLoading) {
    return (
      <div className="rounded-xl bg-white p-8 shadow-lg">
        <div className="h-24 w-24 animate-pulse rounded-full bg-slate-200" />
        <div className="mt-5 h-7 w-64 animate-pulse rounded bg-slate-200" />
        <div className="mt-3 h-4 w-80 animate-pulse rounded bg-slate-100" />
      </div>
    )
  }

  if (!profile) {
    return (
      <div className="rounded-xl bg-white p-8 text-center shadow-lg">
        <p className="text-slate-600">Профіль не знайдено.</p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="grid gap-8 rounded-xl bg-white p-6 shadow-lg md:p-8 lg:grid-cols-[320px_1fr]">
        <section className="border-b border-slate-200 pb-8 lg:border-b-0 lg:border-r lg:pb-0 lg:pr-8">
          <div className="flex flex-col items-center text-center">
            <div className="flex h-28 w-28 items-center justify-center rounded-full bg-blue-600 text-3xl font-bold text-white shadow-md">
              {initials}
            </div>
            <h1 className="mt-5 text-2xl font-bold text-slate-950">{profile.fullName}</h1>
            <p className="mt-1 text-sm text-slate-500">{profile.email}</p>

            <div className="mt-4 flex items-center gap-2 rounded-full bg-amber-50 px-4 py-2 text-sm font-semibold text-amber-700 ring-1 ring-amber-100">
              <Star className="fill-current" size={16} />
              {profile.reviewsCount > 0
                ? `${profile.averageRating} / 5 · відгуків: ${profile.reviewsCount}`
                : 'Відгуків ще немає'}
            </div>
          </div>

          <div className="mt-8 space-y-4">
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Формат роботи
              </p>
              <p className="mt-1 font-medium text-slate-950">
                {formatWorkFormat(profile.workFormat)}
              </p>
            </div>
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Мова
              </p>
              <p className="mt-1 font-medium text-slate-950">
                {profile.preferredLanguage || 'Не вказано'}
              </p>
            </div>
          </div>
        </section>

        <section>
          <h2 className="text-xl font-bold text-slate-950">Навички</h2>
          <div className="mt-4 flex flex-wrap gap-3">
            {profile.skills.length > 0 ? (
              profile.skills.map((skill) => (
                <span
                  className="rounded-full bg-blue-50 px-3 py-2 text-sm font-medium text-blue-700 ring-1 ring-blue-100"
                  key={skill.id}
                >
                  {skill.name}
                  {skill.category && <span className="text-blue-400"> / {skill.category}</span>}
                </span>
              ))
            ) : (
              <p className="text-sm text-slate-500">Навички не вказані.</p>
            )}
          </div>

          <div className="mt-8 grid gap-4 sm:grid-cols-3">
            <div className="rounded-lg border border-slate-200 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Університет
              </p>
              <p className="mt-1 text-sm font-medium text-slate-950">
                {profile.university || 'Не вказано'}
              </p>
            </div>
            <div className="rounded-lg border border-slate-200 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Спеціальність
              </p>
              <p className="mt-1 text-sm font-medium text-slate-950">
                {profile.speciality || 'Не вказано'}
              </p>
            </div>
            <div className="rounded-lg border border-slate-200 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                Курс
              </p>
              <p className="mt-1 text-sm font-medium text-slate-950">
                {profile.course || 'Не вказано'}
              </p>
            </div>
          </div>
        </section>
      </div>

      <section className="rounded-xl bg-white p-6 shadow-lg md:p-8">
        <h2 className="text-xl font-bold text-slate-950">Відгуки</h2>
        <div className="mt-5 space-y-4">
          {profile.reviews.length > 0 ? (
            profile.reviews.map((review) => (
              <article className="rounded-lg border border-slate-200 p-4" key={review.id}>
                <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="font-semibold text-slate-950">{review.reviewerName}</p>
                    <p className="text-sm text-slate-500">{review.projectTitle}</p>
                  </div>
                  <RatingStars rating={review.rating} />
                </div>
                {review.comment && (
                  <p className="mt-3 text-sm leading-6 text-slate-700">{review.comment}</p>
                )}
              </article>
            ))
          ) : (
            <p className="rounded-lg border border-dashed border-slate-300 p-6 text-center text-slate-500">
              Відгуків про цього користувача ще немає.
            </p>
          )}
        </div>
      </section>
    </div>
  )
}

export default ExternalProfile
