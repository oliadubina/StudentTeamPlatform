import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Link as RouterLink, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { z } from 'zod'
import api from '../api/axiosConfig'

const registerSchema = z.object({
  fullName: z.string().min(2, 'Імʼя має містити щонайменше 2 символи'),
  email: z.string().email('Введіть коректну адресу електронної пошти'),
  password: z.string().min(8, 'Пароль має містити щонайменше 8 символів'),
})

function Register() {
  const navigate = useNavigate()
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm({
    defaultValues: {
      fullName: '',
      email: '',
      password: '',
    },
    resolver: zodResolver(registerSchema),
  })

  const onSubmit = async (values) => {
    try {
      await api.post('/api/auth/register', values)
      toast.success('Реєстрація успішна! Увійдіть в систему.')
      navigate('/login')
    } catch (error) {
      toast.error(error.response?.data?.message || 'Сталася помилка')
    }
  }

  return (
    <div className="flex min-h-[calc(100vh-9rem)] items-center justify-center px-4 py-10">
      <div className="w-full max-w-md rounded-xl bg-white p-8 shadow-lg">
        <div className="mb-8">
          <h1 className="text-3xl font-bold tracking-tight text-slate-950">
            Реєстрація
          </h1>
          <p className="mt-2 text-sm text-slate-600">
            Створіть акаунт для роботи зі студентськими командами.
          </p>
        </div>

        <form className="space-y-5" noValidate onSubmit={handleSubmit(onSubmit)}>
          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700" htmlFor="fullName">
              Повне імʼя
            </label>
            <input
              autoComplete="name"
              className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 disabled:cursor-not-allowed disabled:bg-slate-100"
              disabled={isSubmitting}
              id="fullName"
              type="text"
              {...register('fullName')}
            />
            {errors.fullName && (
              <p className="mt-1.5 text-sm text-red-500">{errors.fullName.message}</p>
            )}
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700" htmlFor="email">
              Email
            </label>
            <input
              autoComplete="email"
              className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 disabled:cursor-not-allowed disabled:bg-slate-100"
              disabled={isSubmitting}
              id="email"
              type="email"
              {...register('email')}
            />
            {errors.email && (
              <p className="mt-1.5 text-sm text-red-500">{errors.email.message}</p>
            )}
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700" htmlFor="password">
              Пароль
            </label>
            <input
              autoComplete="new-password"
              className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 disabled:cursor-not-allowed disabled:bg-slate-100"
              disabled={isSubmitting}
              id="password"
              type="password"
              {...register('password')}
            />
            {errors.password && (
              <p className="mt-1.5 text-sm text-red-500">{errors.password.message}</p>
            )}
          </div>

          <button
            className="w-full rounded-lg bg-blue-600 px-4 py-2.5 font-semibold text-white shadow-sm transition hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-300 disabled:cursor-not-allowed disabled:bg-blue-300"
            disabled={isSubmitting}
            type="submit"
          >
            {isSubmitting ? 'Реєстрація...' : 'Зареєструватися'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-slate-600">
          Вже маєте акаунт?{' '}
          <RouterLink className="font-medium text-blue-600 hover:text-blue-700" to="/login">
            Увійти
          </RouterLink>
        </p>
      </div>
    </div>
  )
}

export default Register
