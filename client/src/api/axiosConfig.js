import axios from 'axios'

const baseURL = 'https://localhost:7248'

const api = axios.create({
  baseURL,
})

const getTokenFromResponse = (data) => data?.token || data?.Token || data?.accessToken

const getRefreshTokenFromResponse = (data) =>
  data?.refreshToken || data?.RefreshToken

const clearAuthStorage = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
}

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')

  if (token) {
    config.headers = config.headers || {}
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const refreshToken = localStorage.getItem('refreshToken')
    const isAuthRequest = originalRequest?.url?.includes('/api/auth/')

    if (
      error.response?.status !== 401 ||
      !originalRequest ||
      originalRequest._retry ||
      !refreshToken ||
      isAuthRequest
    ) {
      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      const response = await axios.post(`${baseURL}/api/auth/refresh-token`, {
        refreshToken,
      })
      const token = getTokenFromResponse(response.data)
      const nextRefreshToken = getRefreshTokenFromResponse(response.data)

      if (!token || !nextRefreshToken) {
        clearAuthStorage()
        return Promise.reject(error)
      }

      localStorage.setItem('token', token)
      localStorage.setItem('refreshToken', nextRefreshToken)
      originalRequest.headers = originalRequest.headers || {}
      originalRequest.headers.Authorization = `Bearer ${token}`

      return api(originalRequest)
    } catch (refreshError) {
      clearAuthStorage()
      return Promise.reject(refreshError)
    }
  },
)

export default api
