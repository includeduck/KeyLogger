export type UserSummary = {
  userId: number
  username: string
  email: string
  isEmailVerified: boolean
}

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserSummary
}

export type ProfileResponse = {
  userId: number
  username: string
  email: string
  isEmailVerified: boolean
  createdAt: string
}

export type StoredAuth = {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserSummary
}

const STORAGE_KEY = 'keymapper.auth'

export function loadAuth(): StoredAuth | null {
  const raw = localStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as StoredAuth
  } catch {
    localStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveAuth(auth: StoredAuth): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(auth))
}

export function clearAuth(): void {
  localStorage.removeItem(STORAGE_KEY)
}

export function getAccessToken(): string | null {
  return loadAuth()?.accessToken ?? null
}

export function getRefreshToken(): string | null {
  return loadAuth()?.refreshToken ?? null
}
