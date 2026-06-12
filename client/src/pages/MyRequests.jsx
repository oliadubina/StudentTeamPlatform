import { Link } from 'react-router-dom'
import { toast } from 'react-toastify'
import { Trash2 } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import api from '../api/axiosConfig'

const STATUS_LABELS = {
  Pending: 'Розглядається',
  Accepted: 'Прийнята',
  Rejected: 'Відхилена',
}

const STATUS_CLASSES = {
  Pending: 'bg-amber-50 text-amber-700 ring-amber-100',
  Accepted: 'bg-emerald-50 text-emerald-700 ring-emerald-100',
  Rejected: 'bg-red-50 text-red-700 ring-red-100',
}

const getField = (source, camelName, pascalName, fallback = '') =>
  source?.[camelName] ?? source?.[pascalName] ?? fallback

const normalizeRequest = (request) => ({
  requestId: getField(request, 'requestId', 'RequestId'),
  projectId: getField(request, 'projectId', 'ProjectId'),
  projectTitle: getField(request, 'projectTitle', 'ProjectTitle', 'Без назви'),
  status: getField(request, 'status', 'Status', 'Pending'),
  createdAt: getField(request, 'createdAt', 'CreatedAt'),
})

function MyRequests() {
  const [requests, setRequests] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCancelling, setIsCancelling] = useState(false)

  const fetchRequests = useCallback(async () => {
    setIsLoading(true)

    try {
      const response = await api.get('/api/joinrequest/my-requests')
      setRequests((response.data || []).map(normalizeRequest))
    } catch (error) {
      toast.error(error.response?.data?.message || 'Не вдалося завантажити заявки')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    const timeoutId = setTimeout(fetchRequests, 0)

    return () => clearTimeout(timeoutId)
  }, [fetchRequests])

  const handleCancel = async (requestId) => {
    setIsCancelling(true)

    try {
      await api.delete(`/api/joinrequest/${requestId}/cancel`)
      toast.success('Заявку скасовано')
      await fetchRequests()
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося скасувати заявку')
    } finally {
      setIsCancelling(false)
    }
  }

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-slate-950">
          Мої заявки
        </h1>
        <p className="mt-2 text-slate-600">
          Переглядайте статуси заявок і скасовуйте ті, що ще розглядаються.
        </p>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          <div className="h-24 animate-pulse rounded-xl bg-white shadow-sm" />
          <div className="h-24 animate-pulse rounded-xl bg-white shadow-sm" />
          <div className="h-24 animate-pulse rounded-xl bg-white shadow-sm" />
        </div>
      ) : requests.length > 0 ? (
        <div className="space-y-3">
          {requests.map((request) => (
            <div
              className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm sm:flex-row sm:items-center sm:justify-between"
              key={request.requestId}
            >
              <div>
                <Link
                  className="text-lg font-semibold text-slate-950 hover:text-blue-700"
                  to={`/projects/${request.projectId}`}
                >
                  {request.projectTitle}
                </Link>
                <div className="mt-2 flex flex-wrap items-center gap-2">
                  <span
                    className={`rounded-full px-3 py-1 text-xs font-semibold ring-1 ${
                      STATUS_CLASSES[request.status] || 'bg-slate-100 text-slate-700 ring-slate-200'
                    }`}
                  >
                    {STATUS_LABELS[request.status] || request.status}
                  </span>
                  {request.createdAt && (
                    <span className="text-xs text-slate-500">
                      {new Date(request.createdAt).toLocaleDateString('uk-UA')}
                    </span>
                  )}
                </div>
              </div>

              {request.status === 'Pending' && (
                <button
                  className="inline-flex items-center justify-center gap-2 rounded-lg border border-red-200 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
                  disabled={isCancelling}
                  onClick={() => handleCancel(request.requestId)}
                  type="button"
                >
                  <Trash2 size={16} />
                  Скасувати
                </button>
              )}
            </div>
          ))}
        </div>
      ) : (
        <div className="rounded-xl border border-dashed border-slate-300 bg-white p-10 text-center">
          <p className="font-medium text-slate-700">У вас ще немає заявок.</p>
        </div>
      )}
    </div>
  )
}

export default MyRequests
