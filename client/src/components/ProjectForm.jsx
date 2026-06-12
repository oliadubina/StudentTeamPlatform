import { zodResolver } from '@hookform/resolvers/zod'
import { Plus, Trash2 } from 'lucide-react'
import { useFieldArray, useForm } from 'react-hook-form'
import { z } from 'zod'

const projectSchema = z.object({
  title: z.string().min(3, 'Назва повинна містити принаймні 3 символи'),
  description: z.string().min(10, 'Опис повинен містити принаймні 10 символів'),
  maxContributors: z.coerce.number().min(1, 'Мінімум 1 учасник'),
  projectType: z.coerce.number(),
  workFormat: z.coerce.number(),
  language: z.string().min(1, 'Вкажіть мову проєкту'),
  technologyIds: z.array(z.coerce.number()).min(1, 'Виберіть принаймні одну технологію'),
  roles: z
    .array(
      z.object({
        name: z.string().min(2, 'Назва ролі обов’язкова'),
        slotsCount: z.coerce.number().min(1).max(10),
      })
    )
    .min(1, 'Додайте принаймні одну роль'),
})

const PROJECT_TYPES = [
  { value: 0, label: 'Курсова' },
  { value: 1, label: 'Хакатон' },
  { value: 2, label: 'Стартап' },
]

const WORK_FORMATS = [
  { value: 0, label: 'Онлайн' },
  { value: 1, label: 'Офлайн' },
  { value: 2, label: 'Гібрид' },
]

// Mocked technologies since there is no endpoint for them
const AVAILABLE_TECHNOLOGIES = [
  { id: 1, name: 'React' },
  { id: 2, name: 'Node.js' },
  { id: 3, name: '.NET' },
  { id: 4, name: 'TypeScript' },
  { id: 5, name: 'Python' },
  { id: 6, name: 'PostgreSQL' },
  { id: 7, name: 'Tailwind CSS' },
]

function ProjectForm({ initialData = null, onSubmit, onCancel, isSubmitting }) {
  const {
    register,
    control,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(projectSchema),
    defaultValues: initialData || {
      title: '',
      description: '',
      maxContributors: 1,
      projectType: 0,
      workFormat: 0,
      language: 'Українська',
      technologyIds: [],
      roles: [{ name: '', slotsCount: 1 }],
    },
  })

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'roles',
  })

  const selectedTechIds = watch('technologyIds')

  const toggleTechnology = (id) => {
    const current = [...selectedTechIds]
    const index = current.indexOf(id)
    if (index > -1) {
      current.splice(index, 1)
    } else {
      current.push(id)
    }
    setValue('technologyIds', current, { shouldValidate: true })
  }

  return (
    <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700" htmlFor="title">
            Назва проєкту
          </label>
          <input
            {...register('title')}
            className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-2 text-slate-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
            id="title"
            placeholder="Напр. Платформа для пошуку команд"
            type="text"
          />
          {errors.title && (
            <p className="mt-1 text-xs text-red-500">{errors.title.message}</p>
          )}
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700" htmlFor="description">
            Опис проєкту
          </label>
          <textarea
            {...register('description')}
            className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-2 text-slate-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
            id="description"
            placeholder="Детально опишіть ідею проєкту та цілі..."
            rows={4}
          />
          {errors.description && (
            <p className="mt-1 text-xs text-red-500">{errors.description.message}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700" htmlFor="projectType">
            Тип проєкту
          </label>
          <select
            {...register('projectType')}
            className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-2 text-slate-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
            id="projectType"
          >
            {PROJECT_TYPES.map((type) => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700" htmlFor="workFormat">
            Формат роботи
          </label>
          <select
            {...register('workFormat')}
            className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-2 text-slate-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
            id="workFormat"
          >
            {WORK_FORMATS.map((format) => (
              <option key={format.value} value={format.value}>
                {format.label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700" htmlFor="maxContributors">
            Макс. кількість учасників
          </label>
          <input
            {...register('maxContributors')}
            className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-2 text-slate-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
            id="maxContributors"
            min="1"
            type="number"
          />
          {errors.maxContributors && (
            <p className="mt-1 text-xs text-red-500">{errors.maxContributors.message}</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700" htmlFor="language">
            Мова проєкту
          </label>
          <input
            {...register('language')}
            className="mt-1 block w-full rounded-lg border border-slate-300 px-4 py-2 text-slate-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
            id="language"
            placeholder="Напр. Українська"
            type="text"
          />
          {errors.language && (
            <p className="mt-1 text-xs text-red-500">{errors.language.message}</p>
          )}
        </div>

        <div className="md:col-span-2">
          <label className="block text-sm font-medium text-slate-700">Технології</label>
          <div className="mt-2 flex flex-wrap gap-2">
            {AVAILABLE_TECHNOLOGIES.map((tech) => (
              <button
                key={tech.id}
                className={`rounded-full px-3 py-1 text-xs font-medium transition ${
                  selectedTechIds.includes(tech.id)
                    ? 'bg-blue-600 text-white'
                    : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
                }`}
                onClick={() => toggleTechnology(tech.id)}
                type="button"
              >
                {tech.name}
              </button>
            ))}
          </div>
          {errors.technologyIds && (
            <p className="mt-1 text-xs text-red-500">{errors.technologyIds.message}</p>
          )}
        </div>

        <div className="md:col-span-2">
          <div className="flex items-center justify-between">
            <label className="block text-sm font-medium text-slate-700">Ролі в команді</label>
            <button
              className="inline-flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-700"
              onClick={() => append({ name: '', slotsCount: 1 })}
              type="button"
            >
              <Plus size={14} /> Додати роль
            </button>
          </div>
          <div className="mt-2 space-y-3">
            {fields.map((field, index) => (
              <div key={field.id} className="flex gap-3">
                <div className="flex-1">
                  <input
                    {...register(`roles.${index}.name`)}
                    className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
                    placeholder="Назва ролі (напр. Backend Developer)"
                  />
                  {errors.roles?.[index]?.name && (
                    <p className="mt-1 text-xs text-red-500">
                      {errors.roles[index].name.message}
                    </p>
                  )}
                </div>
                <div className="w-24">
                  <input
                    {...register(`roles.${index}.slotsCount`)}
                    className="w-full rounded-lg border border-slate-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none"
                    placeholder="Місця"
                    type="number"
                  />
                  {errors.roles?.[index]?.slotsCount && (
                    <p className="mt-1 text-xs text-red-500">
                      {errors.roles[index].slotsCount.message}
                    </p>
                  )}
                </div>
                {fields.length > 1 && (
                  <button
                    className="text-slate-400 hover:text-red-500"
                    onClick={() => remove(index)}
                    type="button"
                  >
                    <Trash2 size={18} />
                  </button>
                )}
              </div>
            ))}
          </div>
          {errors.roles?.root && (
            <p className="mt-1 text-xs text-red-500">{errors.roles.root.message}</p>
          )}
        </div>
      </div>

      <div className="flex justify-end gap-3 border-t border-slate-200 pt-6">
        <button
          className="rounded-lg bg-slate-100 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-200"
          onClick={onCancel}
          type="button"
        >
          Скасувати
        </button>
        <button
          className="inline-flex items-center justify-center rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
          disabled={isSubmitting}
          type="submit"
        >
          {isSubmitting ? 'Збереження...' : initialData ? 'Оновити проєкт' : 'Створити проєкт'}
        </button>
      </div>
    </form>
  )
}

export default ProjectForm
