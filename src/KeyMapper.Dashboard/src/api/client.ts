import { clearAuth, getAccessToken, getRefreshToken, saveAuth, type AuthResponse, type ProfileResponse } from '../auth/tokenStorage'

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

type RequestOptions = {
  method?: string
  body?: unknown
  auth?: boolean
}

let refreshPromise: Promise<boolean> | null = null

async function refreshAccessToken(): Promise<boolean> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    return false
  }

  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  })

  if (!response.ok) {
    clearAuth()
    return false
  }

  const data = (await response.json()) as AuthResponse
  saveAuth({
    accessToken: data.accessToken,
    refreshToken: data.refreshToken,
    expiresAt: data.expiresAt,
    user: data.user,
  })

  return true
}

async function ensureRefreshed(): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = refreshAccessToken().finally(() => {
      refreshPromise = null
    })
  }

  return refreshPromise
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  }

  if (options.auth !== false) {
    const token = getAccessToken()
    if (token) {
      headers.Authorization = `Bearer ${token}`
    }
  }

  const response = await fetch(path, {
    method: options.method ?? 'GET',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  })

  if (response.status === 401 && options.auth !== false) {
    const refreshed = await ensureRefreshed()
    if (refreshed) {
      return apiRequest<T>(path, options)
    }

    clearAuth()
    window.location.assign('/login')
    throw new ApiError('Session expired.', 401)
  }

  const payload = await response.json().catch(() => ({}))

  if (!response.ok) {
    const message =
      typeof payload.error === 'string'
        ? payload.error
        : typeof payload.message === 'string'
          ? payload.message
          : 'Request failed.'
    throw new ApiError(message, response.status)
  }

  return payload as T
}

export async function loginRequest(usernameOrEmail: string, password: string): Promise<AuthResponse> {
  return apiRequest<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: { usernameOrEmail, password },
    auth: false,
  })
}

export async function registerRequest(
  username: string,
  email: string,
  password: string,
  acceptTerms: boolean,
): Promise<{ message: string }> {
  return apiRequest<{ message: string }>('/api/auth/register', {
    method: 'POST',
    body: { username, email, password, acceptTerms },
    auth: false,
  })
}

export async function logoutRequest(refreshToken: string): Promise<void> {
  await apiRequest('/api/auth/logout', {
    method: 'POST',
    body: { refreshToken },
  })
}

export async function forgotPasswordRequest(email: string): Promise<{ message: string }> {
  return apiRequest<{ message: string }>('/api/auth/forgot-password', {
    method: 'POST',
    body: { email },
    auth: false,
  })
}

export async function resetPasswordRequest(token: string, newPassword: string): Promise<{ message: string }> {
  return apiRequest<{ message: string }>('/api/auth/reset-password', {
    method: 'POST',
    body: { token, newPassword },
    auth: false,
  })
}

export async function verifyEmailRequest(token: string): Promise<{ message: string }> {
  return apiRequest<{ message: string }>(`/api/auth/verify-email?token=${encodeURIComponent(token)}`, {
    auth: false,
  })
}

export async function getProfileRequest(): Promise<ProfileResponse> {
  return apiRequest<ProfileResponse>('/api/profile')
}

export async function changePasswordRequest(
  currentPassword: string,
  newPassword: string,
): Promise<{ message: string }> {
  return apiRequest('/api/profile/password', {
    method: 'PUT',
    body: { currentPassword, newPassword },
  })
}
