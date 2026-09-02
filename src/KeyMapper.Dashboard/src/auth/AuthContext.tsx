import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { loginRequest, logoutRequest, registerRequest } from '../api/client'
import {
  clearAuth,
  loadAuth,
  saveAuth,
  type StoredAuth,
  type UserSummary,
} from './tokenStorage'

type AuthContextValue = {
  user: UserSummary | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (usernameOrEmail: string, password: string) => Promise<void>
  register: (
    username: string,
    email: string,
    password: string,
    acceptTerms: boolean,
  ) => Promise<string>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuth] = useState<StoredAuth | null>(() => loadAuth())
  const [isLoading, setIsLoading] = useState(false)

  const login = useCallback(async (usernameOrEmail: string, password: string) => {
    setIsLoading(true)
    try {
      const response = await loginRequest(usernameOrEmail, password)
      const stored: StoredAuth = {
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        expiresAt: response.expiresAt,
        user: response.user,
      }
      saveAuth(stored)
      setAuth(stored)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const register = useCallback(
    async (username: string, email: string, password: string, acceptTerms: boolean) => {
      setIsLoading(true)
      try {
        const response = await registerRequest(username, email, password, acceptTerms)
        return response.message
      } finally {
        setIsLoading(false)
      }
    },
    [],
  )

  const logout = useCallback(async () => {
    const refreshToken = auth?.refreshToken

    if (refreshToken) {
      try {
        await logoutRequest(refreshToken)
      } catch {
        // Ignore logout API errors; local session will still be cleared.
      }
    }

    clearAuth()
    setAuth(null)
  }, [auth?.refreshToken])

  const value = useMemo<AuthContextValue>(
    () => ({
      user: auth?.user ?? null,
      isAuthenticated: auth !== null,
      isLoading,
      login,
      register,
      logout,
    }),
    [auth, isLoading, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }

  return context
}
