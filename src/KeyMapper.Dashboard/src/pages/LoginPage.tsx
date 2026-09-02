import { useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { AuthLinkRow, LoginIconButton } from '../components/AppShell'
import { AuthLayout, AuthSwitch, FormError } from '../components/AuthLayout'

export function LoginPage() {
  const { login, isLoading } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [usernameOrEmail, setUsernameOrEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)

  const redirectTo =
    (location.state as { from?: string } | null)?.from ?? '/dashboard'

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)

    try {
      await login(usernameOrEmail, password)
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to sign in.')
    }
  }

  const canSubmit = usernameOrEmail.trim() !== '' && password !== '' && !isLoading

  return (
    <AuthLayout title="Sign in">
      <form className="auth-form" onSubmit={handleSubmit}>
        <FormError message={error} />

        <label>
          Username or email
          <input
            type="text"
            name="usernameOrEmail"
            autoComplete="username"
            value={usernameOrEmail}
            onChange={(event) => setUsernameOrEmail(event.target.value)}
          />
        </label>

        <label>
          Password
          <input
            type="password"
            name="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </label>

        <AuthLinkRow />

        <button type="submit" className="primary-action" disabled={!canSubmit}>
          <LoginIconButton />
          {isLoading ? 'Signing in...' : 'Sign in'}
        </button>
      </form>

      <AuthSwitch isLogin />
    </AuthLayout>
  )
}
