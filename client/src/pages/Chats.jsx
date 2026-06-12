import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { LogOut, Send, Trash2 } from 'lucide-react'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { toast } from 'react-toastify'
import api from '../api/axiosConfig'

const getField = (source, camelName, pascalName, fallback = '') =>
  source?.[camelName] ?? source?.[pascalName] ?? fallback

const decodeJwtPayload = (token) => {
  try {
    const [, payload] = token.split('.')
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(
      decodeURIComponent(
        atob(base64)
          .split('')
          .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`)
          .join(''),
      ),
    )
  } catch {
    return null
  }
}

const getCurrentUserId = () => {
  const token = localStorage.getItem('token')
  const payload = token ? decodeJwtPayload(token) : null

  return Number(
    payload?.nameid ||
      payload?.sub ||
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
  )
}

const normalizeChat = (chat) => ({
  projectId: getField(chat, 'projectId', 'ProjectId'),
  title: getField(chat, 'title', 'Title', 'Без назви'),
  isAuthor: getField(chat, 'isAuthor', 'IsAuthor', false),
  lastMessageAt: getField(chat, 'lastMessageAt', 'LastMessageAt', null),
})

const normalizeMessage = (message) => ({
  id: getField(message, 'id', 'Id', `${Date.now()}-${Math.random()}`),
  projectId: getField(message, 'projectId', 'ProjectId'),
  senderId: getField(message, 'senderId', 'SenderId'),
  senderName: getField(message, 'senderName', 'SenderName', 'Користувач'),
  text: getField(message, 'text', 'Text'),
  sentAt: getField(message, 'sentAt', 'SentAt', new Date().toISOString()),
})

const normalizeProjectDetails = (project) => ({
  id: getField(project, 'id', 'Id'),
  title: getField(project, 'title', 'Title', 'Без назви'),
  isAuthor: getField(project, 'isAuthor', 'IsAuthor', false),
  isContributor: getField(project, 'isContributor', 'IsContributor', false),
  contributors: (getField(project, 'contributors', 'Contributors', []) || []).map((contributor) => ({
    id: getField(contributor, 'id', 'Id'),
    fullName: getField(contributor, 'fullName', 'FullName', 'Учасник'),
    email: getField(contributor, 'email', 'Email'),
  })),
})

function Chats() {
  const [chats, setChats] = useState([])
  const [activeProjectId, setActiveProjectId] = useState(null)
  const [projectDetails, setProjectDetails] = useState(null)
  const [messages, setMessages] = useState([])
  const [messageText, setMessageText] = useState('')
  const [isLoadingChats, setIsLoadingChats] = useState(true)
  const [isLoadingMessages, setIsLoadingMessages] = useState(false)
  const [isSending, setIsSending] = useState(false)
  const connectionRef = useRef(null)
  const activeProjectIdRef = useRef(null)
  const messagesEndRef = useRef(null)

  const currentUserId = useMemo(() => getCurrentUserId(), [])
  const activeChat = useMemo(
    () => chats.find((chat) => chat.projectId === activeProjectId),
    [activeProjectId, chats],
  )

  useEffect(() => {
    activeProjectIdRef.current = activeProjectId
  }, [activeProjectId])

  const fetchChats = useCallback(async () => {
    setIsLoadingChats(true)

    try {
      const response = await api.get('/api/project/chats')
      const normalizedChats = (response.data || []).map(normalizeChat)

      setChats(normalizedChats)
      setActiveProjectId((current) => current || normalizedChats[0]?.projectId || null)
    } catch (error) {
      toast.error(error.response?.data?.message || 'Не вдалося завантажити чати')
    } finally {
      setIsLoadingChats(false)
    }
  }, [])

  const fetchProjectData = useCallback(async (projectId) => {
    if (!projectId) {
      return
    }

    setIsLoadingMessages(true)

    try {
      const [historyResponse, projectResponse] = await Promise.all([
        api.get(`/api/project/${projectId}/chat-history`),
        api.get(`/api/project/${projectId}`),
      ])

      setMessages((historyResponse.data || []).map(normalizeMessage))
      setProjectDetails(normalizeProjectDetails(projectResponse.data))
    } catch (error) {
      toast.error(error.response?.data?.message || 'Не вдалося завантажити чат')
    } finally {
      setIsLoadingMessages(false)
    }
  }, [])

  useEffect(() => {
    const timeoutId = setTimeout(fetchChats, 0)

    return () => clearTimeout(timeoutId)
  }, [fetchChats])

  useEffect(() => {
    if (!activeProjectId) {
      const timeoutId = setTimeout(() => {
        setMessages([])
        setProjectDetails(null)
      }, 0)

      return () => clearTimeout(timeoutId)
    }

    const timeoutId = setTimeout(() => fetchProjectData(activeProjectId), 0)

    return () => clearTimeout(timeoutId)
  }, [activeProjectId, fetchProjectData])

  useEffect(() => {
    const token = localStorage.getItem('token')
    let isCancelled = false

    if (!token) {
      return undefined
    }

    const connection = new HubConnectionBuilder()
      .withUrl('https://localhost:7248/chathub', {
        accessTokenFactory: () => localStorage.getItem('token') || '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('ReceiveMessage', (message) => {
      const normalizedMessage = normalizeMessage(message)

      setMessages((current) =>
        normalizedMessage.projectId === activeProjectIdRef.current
          ? [...current, normalizedMessage]
          : current,
      )
    })

    connection
      .start()
      .then(() => {
        if (!isCancelled) {
          connectionRef.current = connection
        }
      })
      .catch(() => {
        if (!isCancelled) {
          toast.error('Не вдалося підключитися до чату')
        }
      })

    return () => {
      isCancelled = true
      connection.stop()
      if (connectionRef.current === connection) {
        connectionRef.current = null
      }
    }
  }, [])

  useEffect(() => {
    const connection = connectionRef.current

    if (!connection || connection.state !== HubConnectionState.Connected || !activeProjectId) {
      return
    }

    connection.invoke('JoinProjectChat', String(activeProjectId)).catch((error) => {
      if (connection.state === HubConnectionState.Connected) {
        toast.error(error?.message || 'Не вдалося приєднатися до чату проєкту')
      }
    })
  }, [activeProjectId])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const handleSend = async (event) => {
    event.preventDefault()

    const text = messageText.trim()
    const connection = connectionRef.current

    if (!text || !activeProjectId || !connection) {
      return
    }

    setIsSending(true)

    try {
      await connection.invoke('SendMessage', activeProjectId, text)
      setMessageText('')
    } catch {
      toast.error('Не вдалося надіслати повідомлення')
    } finally {
      setIsSending(false)
    }
  }

  const handleRemoveContributor = async (studentId) => {
    if (!activeProjectId) {
      return
    }

    try {
      await api.delete(`/api/project/${activeProjectId}/contributors/${studentId}`)
      toast.success(studentId === currentUserId ? 'Ви покинули команду' : 'Учасника видалено')
      await fetchChats()

      if (studentId === currentUserId) {
        setActiveProjectId(null)
        setProjectDetails(null)
        setMessages([])
      } else {
        await fetchProjectData(activeProjectId)
      }
    } catch (error) {
      toast.error(error.response?.data?.message || error.response?.data || 'Не вдалося виконати дію')
    }
  }

  return (
    <div className="grid h-[calc(100vh-9rem)] min-h-0 gap-6 overflow-hidden lg:grid-cols-[320px_1fr]">
      <aside className="min-h-0 overflow-y-auto rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
        <div className="mb-4">
          <h1 className="text-2xl font-bold text-slate-950">Чати</h1>
          <p className="mt-1 text-sm text-slate-500">
            Доступні після прийняття в команду або для ваших проєктів.
          </p>
        </div>

        {isLoadingChats ? (
          <div className="space-y-2">
            <div className="h-16 animate-pulse rounded-lg bg-slate-100" />
            <div className="h-16 animate-pulse rounded-lg bg-slate-100" />
            <div className="h-16 animate-pulse rounded-lg bg-slate-100" />
          </div>
        ) : chats.length > 0 ? (
          <div className="space-y-2">
            {chats.map((chat) => (
              <button
                className={`w-full rounded-lg px-3 py-3 text-left transition ${
                  chat.projectId === activeProjectId
                    ? 'bg-blue-50 text-blue-800 ring-1 ring-blue-100'
                    : 'hover:bg-slate-100'
                }`}
                key={chat.projectId}
                onClick={() => setActiveProjectId(chat.projectId)}
                type="button"
              >
                <p className="font-medium">{chat.title}</p>
                <p className="mt-1 text-xs text-slate-500">
                  {chat.isAuthor ? 'Ви автор' : 'Ви учасник'}
                </p>
              </button>
            ))}
          </div>
        ) : (
          <p className="rounded-lg border border-dashed border-slate-300 p-4 text-center text-sm text-slate-500">
            У вас поки немає активних чатів.
          </p>
        )}
      </aside>

      <section className="flex min-h-0 flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        {activeChat ? (
          <>
            <header className="shrink-0 border-b border-slate-200 p-4">
              <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                <div>
                  <h2 className="text-xl font-bold text-slate-950">{activeChat.title}</h2>
                  <Link
                    className="mt-1 inline-block text-sm font-medium text-blue-600 hover:text-blue-700"
                    to={`/projects/${activeProjectId}`}
                  >
                    Перейти до деталей проєкту
                  </Link>
                </div>

                {projectDetails?.isContributor && (
                  <button
                    className="inline-flex items-center justify-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-50"
                    onClick={() => handleRemoveContributor(currentUserId)}
                    type="button"
                  >
                    <LogOut size={16} />
                    Покинути команду
                  </button>
                )}
              </div>
            </header>

            {projectDetails?.isAuthor && projectDetails.contributors.length > 0 && (
              <div className="shrink-0 border-b border-slate-200 bg-slate-50 p-4">
                <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
                  Учасники
                </p>
                <div className="flex flex-wrap gap-2">
                  {projectDetails.contributors.map((contributor) => (
                    <span
                      className="inline-flex items-center gap-2 rounded-full bg-white px-3 py-1.5 text-sm text-slate-700 ring-1 ring-slate-200"
                      key={contributor.id}
                    >
                      {contributor.fullName}
                      <button
                        className="rounded-full p-0.5 text-red-500 hover:bg-red-50"
                        onClick={() => handleRemoveContributor(contributor.id)}
                        title="Видалити з команди"
                        type="button"
                      >
                        <Trash2 size={13} />
                      </button>
                    </span>
                  ))}
                </div>
              </div>
            )}

            <div className="min-h-0 flex-1 space-y-3 overflow-y-auto p-4">
              {isLoadingMessages ? (
                <div className="space-y-3">
                  <div className="h-14 w-2/3 animate-pulse rounded-lg bg-slate-100" />
                  <div className="ml-auto h-14 w-2/3 animate-pulse rounded-lg bg-blue-100" />
                </div>
              ) : messages.length > 0 ? (
                messages.map((message) => {
                  const isMine = Number(message.senderId) === Number(currentUserId)

                  return (
                    <div
                      className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}
                      key={message.id}
                    >
                      <div
                        className={`max-w-[78%] rounded-2xl px-4 py-2 ${
                          isMine ? 'bg-blue-600 text-white' : 'bg-slate-100 text-slate-900'
                        }`}
                      >
                        {!isMine && (
                          <p className="mb-1 text-xs font-semibold text-slate-500">
                            {message.senderName}
                          </p>
                        )}
                        <p className="whitespace-pre-wrap text-sm leading-6">{message.text}</p>
                        <p className={`mt-1 text-[11px] ${isMine ? 'text-blue-100' : 'text-slate-400'}`}>
                          {new Date(message.sentAt).toLocaleTimeString('uk-UA', {
                            hour: '2-digit',
                            minute: '2-digit',
                          })}
                        </p>
                      </div>
                    </div>
                  )
                })
              ) : (
                <p className="rounded-lg border border-dashed border-slate-300 p-6 text-center text-sm text-slate-500">
                  Повідомлень ще немає. Почніть розмову.
                </p>
              )}
              <div ref={messagesEndRef} />
            </div>

            <form className="shrink-0 border-t border-slate-200 p-4" onSubmit={handleSend}>
              <div className="flex gap-2">
                <input
                  className="flex-1 rounded-lg border border-slate-300 px-3 py-2 text-slate-950 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-200"
                  onChange={(event) => setMessageText(event.target.value)}
                  placeholder="Напишіть повідомлення..."
                  value={messageText}
                />
                <button
                  className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 font-semibold text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-blue-300"
                  disabled={isSending || !messageText.trim()}
                  type="submit"
                >
                  <Send size={16} />
                  Надіслати
                </button>
              </div>
            </form>
          </>
        ) : (
          <div className="flex flex-1 items-center justify-center p-8 text-center">
            <p className="text-slate-500">Оберіть чат зі списку.</p>
          </div>
        )}
      </section>
    </div>
  )
}

export default Chats
