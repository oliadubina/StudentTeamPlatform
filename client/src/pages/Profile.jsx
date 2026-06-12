import { Plus, Save, UserRound, X } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { toast } from 'react-toastify'
import api from '../api/axiosConfig'

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

const getField = (source, camelName, pascalName, fallback = '') =>
  source?.[camelName] ?? source?.[pascalName] ?? fallback

const normalizeSkill = (skill, index) => ({
  id: getField(skill, 'id', 'Id', 0),
  localId: getField(skill, 'id', 'Id', `new-${index}-${Date.now()}`),
  name: getField(skill, 'name', 'Name'),
  category: getField(skill, 'category', 'Category'),
})

const normalizeProfile = (data) => ({
  fullName: getField(data, 'fullName', 'FullName'),
  email: getField(data, 'emailAddress', 'EmailAddress', getField(data, 'email', 'Email')),
  university: getField(data, 'university', 'University'),
  speciality: getField(data, 'speciality', 'Speciality'),
  course: getField(data, 'course', 'Course', 0),
  workFormat: getField(data, 'workFormat', 'WorkFormat'),
  preferredLanguage: getField(data, 'preferredLanguage', 'PreferredLanguage'),
  skills: (getField(data, 'skills', 'Skills', []) || []).map(normalizeSkill),
})

const buildUpdatePayload = (profile) => ({
  fullName: profile.fullName,
  university: profile.university,
  speciality: profile.speciality,
  course: Number(profile.course) || 0,
  workFormat: profile.workFormat || null,
  preferredLanguage: profile.preferredLanguage || '',
  skills: profile.skills.map((skill) => ({
    id: Number(skill.id) || 0,
    name: skill.name,
    category: skill.category,
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

const formatWorkFormat = (value) =>
  WORK_FORMAT_OPTIONS.find((option) => option.value === value)?.label || 'Не вказано'

const formatLanguage = (value) =>
  LANGUAGE_OPTIONS.find((option) => option.value === value)?.label || value || 'Не вказано'

function ProfileSkeleton() {
  return (
    <div className="grid gap-8 rounded-xl bg-white p-8 shadow-lg lg:grid-cols-[320px_1fr]">
      <div className="space-y-5">
        <div className="mx-auto h-28 w-28 animate-pulse rounded-full bg-slate-200" />
        <div className="mx-auto h-6 w-48 animate-pulse rounded bg-slate-200" />
        <div className="mx-auto h-4 w-56 animate-pulse rounded bg-slate-200" />
        <div className="space-y-3 pt-4">
          <div className="h-16 animate-pulse rounded-lg bg-slate-100" />
          <div className="h-16 animate-pulse rounded-lg bg-slate-100" />
        </div>
      </div>
      <div className="space-y-4">
        <div className="h-7 w-40 animate-pulse rounded bg-slate-200" />
        <div className="flex flex-wrap gap-3">
          <div className="h-9 w-24 animate-pulse rounded-full bg-slate-100" />
          <div className="h-9 w-32 animate-pulse rounded-full bg-slate-100" />
          <div className="h-9 w-20 animate-pulse rounded-full bg-slate-100" />
        </div>
        <div className="mt-8 h-11 w-44 animate-pulse rounded-lg bg-slate-200" />
      </div>
    </div>
  )
}

function Profile() {
  const [profile, setProfile] = useState(null)
  const [draft, setDraft] = useState(null)
  const [isEditing, setIsEditing] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [newSkillName, setNewSkillName] = useState('')
  const [newSkillCategory, setNewSkillCategory] = useState('')

  const initials = useMemo(() => getInitials(profile?.fullName || ''), [profile])

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await api.get('/api/profile/profile')
        const normalizedProfile = normalizeProfile(response.data)
        setProfile(normalizedProfile)
        setDraft(normalizedProfile)
      } catch (error) {
        toast.error(error.response?.data?.message || 'Не вдалося завантажити профіль')
      } finally {
        setIsLoading(false)
      }
    }

    fetchProfile()
  }, [])

  const validateDraft = () => {
    if (!draft.university?.trim()) {
      toast.error('Вкажіть університет перед збереженням профілю')
      return false
    }

    if (!draft.speciality?.trim()) {
      toast.error('Вкажіть спеціальність перед збереженням профілю')
      return false
    }

    if (!Number(draft.course)) {
      toast.error('Вкажіть курс перед збереженням профілю')
      return false
    }

    return true
  }

  const saveProfile = async (nextProfile) => {
    setIsSaving(true)

    try {
      const payload = buildUpdatePayload(nextProfile)
      const response = await api.put('/api/profile/update-profile', payload)
      const updatedProfile = normalizeProfile({ ...nextProfile, ...response.data })

      setProfile(updatedProfile)
      setDraft(updatedProfile)
      toast.success('Профіль успішно оновлено!')
      return true
    } catch (error) {
      toast.error(error.response?.data?.message || 'Не вдалося оновити профіль')
      return false
    } finally {
      setIsSaving(false)
    }
  }

  const handleEdit = () => {
    setDraft(profile)
    setIsEditing(true)
  }

  const handleCancel = () => {
    setDraft(profile)
    setIsEditing(false)
    setNewSkillName('')
    setNewSkillCategory('')
  }

  const handleSave = async () => {
    if (!validateDraft()) {
      return
    }

    const isUpdated = await saveProfile(draft)

    if (isUpdated) {
      setIsEditing(false)
    }
  }

  const handleAddSkill = () => {
    const name = newSkillName.trim()
    const category = newSkillCategory.trim()

    if (!name || !category) {
      toast.error('Вкажіть назву та категорію навички')
      return
    }

    setDraft((current) => ({
      ...current,
      skills: [
        ...current.skills,
        {
          id: 0,
          localId: `new-${Date.now()}`,
          name,
          category,
        },
      ],
    }))
    setNewSkillName('')
    setNewSkillCategory('')
  }

  const handleRemoveSkill = (skillToRemove) => {
    setDraft((current) => ({
      ...current,
      skills: current.skills.filter((skill) => skill.localId !== skillToRemove.localId),
    }))
  }

  if (isLoading) {
    return <ProfileSkeleton />
  }

  if (!profile || !draft) {
    return (
      <div className="rounded-xl bg-white p-8 text-center shadow-lg">
        <p className="text-slate-600">Профіль не знайдено.</p>
      </div>
    )
  }

  return (
    <div className="grid gap-8 rounded-xl bg-white p-6 shadow-lg md:p-8 lg:grid-cols-[320px_1fr]">
      <section className="border-b border-slate-200 pb-8 lg:border-b-0 lg:border-r lg:pb-0 lg:pr-8">
        <div className="flex flex-col items-center text-center">
          <div className="flex h-28 w-28 items-center justify-center rounded-full bg-blue-600 text-3xl font-bold text-white shadow-md">
            {initials}
          </div>

          <h1 className="mt-5 text-2xl font-bold text-slate-950">{profile.fullName}</h1>
          <p className="mt-1 text-sm text-slate-500">{profile.email}</p>
        </div>

        <div className="mt-8 space-y-4">
          <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Формат роботи
            </p>
            {isEditing ? (
              <select
                className="mt-2 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                onChange={(event) =>
                  setDraft((current) => ({ ...current, workFormat: event.target.value }))
                }
                value={draft.workFormat || ''}
              >
                <option value="">Не вказано</option>
                {WORK_FORMAT_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            ) : (
              <p className="mt-1 font-medium text-slate-950">
                {formatWorkFormat(profile.workFormat)}
              </p>
            )}
          </div>

          <div className="rounded-lg border border-slate-200 bg-slate-50 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Мова
            </p>
            {isEditing ? (
              <select
                className="mt-2 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                onChange={(event) =>
                  setDraft((current) => ({ ...current, preferredLanguage: event.target.value }))
                }
                value={draft.preferredLanguage || ''}
              >
                <option value="">Не вказано</option>
                {LANGUAGE_OPTIONS.map((language) => (
                  <option key={language.value} value={language.value}>
                    {language.label}
                  </option>
                ))}
              </select>
            ) : (
              <p className="mt-1 font-medium text-slate-950">
                {formatLanguage(profile.preferredLanguage)}
              </p>
            )}
          </div>
        </div>
      </section>

      <section>
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <div className="flex items-center gap-2 text-slate-950">
              <UserRound size={22} />
              <h2 className="text-xl font-bold">Мої навички</h2>
            </div>
            <p className="mt-1 text-sm text-slate-500">
              Технології та напрями, у яких ви готові працювати.
            </p>
          </div>

          {isEditing ? (
            <div className="flex gap-2">
              <button
                className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100"
                disabled={isSaving}
                onClick={handleCancel}
                type="button"
              >
                Скасувати
              </button>
              <button
                className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
                disabled={isSaving}
                onClick={handleSave}
                type="button"
              >
                <Save size={16} />
                Зберегти
              </button>
            </div>
          ) : (
            <button
              className="rounded-lg bg-slate-950 px-4 py-2 text-sm font-medium text-white hover:bg-slate-800"
              onClick={handleEdit}
              type="button"
            >
              Редагувати профіль
            </button>
          )}
        </div>

        <div className="mt-6 flex flex-wrap gap-3">
          {draft.skills.length > 0 ? (
            draft.skills.map((skill, index) => (
              <span
                className="inline-flex items-center gap-2 rounded-full bg-blue-50 px-3 py-2 text-sm font-medium text-blue-700 ring-1 ring-blue-100"
                key={`${skill.localId}-${index}`}
              >
                {skill.name}
                <span className="text-blue-400">/{skill.category}</span>
                {isEditing && (
                  <button
                    className="rounded-full p-0.5 text-blue-500 hover:bg-blue-100 hover:text-blue-800"
                    disabled={isSaving}
                    onClick={() => handleRemoveSkill(skill)}
                    type="button"
                  >
                    <X size={14} />
                  </button>
                )}
              </span>
            ))
          ) : (
            <p className="text-sm text-slate-500">Навички ще не додані.</p>
          )}
        </div>

        {isEditing && (
          <div className="mt-8 rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h3 className="font-semibold text-slate-950">Додати навичку</h3>
            <div className="mt-4 grid gap-3 sm:grid-cols-[1fr_1fr_auto]">
              <input
                className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                onChange={(event) => setNewSkillName(event.target.value)}
                placeholder="React"
                type="text"
                value={newSkillName}
              />
              <input
                className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                onChange={(event) => setNewSkillCategory(event.target.value)}
                placeholder="Frontend"
                type="text"
                value={newSkillCategory}
              />
              <button
                className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
                disabled={isSaving}
                onClick={handleAddSkill}
                type="button"
              >
                <Plus size={16} />
                Додати
              </button>
            </div>
          </div>
        )}

        <div className="mt-8 grid gap-4 sm:grid-cols-3">
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Університет
            </p>
            {isEditing ? (
              <input
                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                onChange={(event) =>
                  setDraft((current) => ({ ...current, university: event.target.value }))
                }
                value={draft.university || ''}
              />
            ) : (
              <p className="mt-1 text-sm font-medium text-slate-950">
                {profile.university || 'Не вказано'}
              </p>
            )}
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Спеціальність
            </p>
            {isEditing ? (
              <input
                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                onChange={(event) =>
                  setDraft((current) => ({ ...current, speciality: event.target.value }))
                }
                value={draft.speciality || ''}
              />
            ) : (
              <p className="mt-1 text-sm font-medium text-slate-950">
                {profile.speciality || 'Не вказано'}
              </p>
            )}
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
              Курс
            </p>
            {isEditing ? (
              <input
                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                min="1"
                onChange={(event) =>
                  setDraft((current) => ({ ...current, course: event.target.value }))
                }
                type="number"
                value={draft.course || ''}
              />
            ) : (
              <p className="mt-1 text-sm font-medium text-slate-950">
                {profile.course || 'Не вказано'}
              </p>
            )}
          </div>
        </div>
      </section>
    </div>
  )
}

export default Profile
