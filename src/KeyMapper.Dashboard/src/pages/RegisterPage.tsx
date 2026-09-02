import { useState, type FormEvent } from 'react'
import { UserPlus } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { AuthLayout, AuthSwitch, FormError, FormSuccess } from '../components/AuthLayout'

export function RegisterPage() {
  const { register, isLoading } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [acceptTerms, setAcceptTerms] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setSuccess(null)

    try {
      const message = await register(username, email, password, acceptTerms)
      setSuccess(`${message} Check the API console for the verification link.`)
      setTimeout(() => navigate('/login'), 2500)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to register.')
    }
  }

  const canSubmit =
    username.trim() !== '' &&
    email.trim() !== '' &&
    password !== '' &&
    acceptTerms &&
    !isLoading

  return (
    <AuthLayout title="Create account">
      <form className="auth-form" onSubmit={handleSubmit}>
        <FormError message={error} />
        <FormSuccess message={success} />

        <label>
          Username
          <input
            type="text"
            name="username"
            autoComplete="username"
            value={username}
            onChange={(event) => setUsername(event.target.value)}
          />
        </label>

        <label>
          Email
          <input
            type="email"
            name="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </label>

        <label>
          Password
          <input
            type="password"
            name="password"
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </label>

        <label className="terms-row">
          <input
            type="checkbox"
            name="terms"
            checked={acceptTerms}
            onChange={(event) => setAcceptTerms(event.target.checked)}
          />
          <span>I accept the terms of service</span>
        </label>

        <button type="submit" className="primary-action" disabled={!canSubmit}>
          <UserPlus size={18} />
          {isLoading ? 'Creating account...' : 'Register'}
        </button>
      </form>

      <AuthSwitch isLogin={false} />
    </AuthLayout>
  )
}
