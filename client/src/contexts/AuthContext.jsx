import { useCallback, useMemo, useState } from 'react'
import { AuthContext } from './authContext'

const decodeJwtPayload = (token) => {
  try {
    const [, payload] = token.split('.')

    if (!payload) {
      return null
    }

    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    const decodedPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join(''),
    )

    return JSON.parse(decodedPayload)
  } catch {
    return null
  }
}

const getUserFromToken = (token) => {
  const payload = decodeJwtPayload(token)

  if (!payload) {
    return null
  }

  if (payload.exp && payload.exp * 1000 <= Date.now()) {
    return null
  }

  return payload
}

const getInitialUser = () => {
  const token = localStorage.getItem('token')

  if (!token) {
    return null
  }

  const user = getUserFromToken(token)

  if (!user) {
    localStorage.removeItem('token')
  }

  return user
}

export function AuthProvider({ children }) {
  const [currentUser, setCurrentUser] = useState(getInitialUser)

  const login = useCallback((token) => {
    if (!token || typeof token !== 'string') {
      localStorage.removeItem('token')
      setCurrentUser(null)
      return null
    }

    localStorage.setItem('token', token)
    const user = getUserFromToken(token) || { token }

    setCurrentUser(user)
    return user
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem('token')
    setCurrentUser(null)
  }, [])

  const value = useMemo(
    () => ({
      currentUser,
      isAuthenticated: Boolean(currentUser),
      login,
      logout,
    }),
    [currentUser, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
