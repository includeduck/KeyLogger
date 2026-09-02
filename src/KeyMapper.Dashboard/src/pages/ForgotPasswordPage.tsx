import { useState, type FormEvent } from 'react'
import { NavLink } from 'react-router-dom'
import { ApiError, forgotPasswordRequest } from '../api/client'
import { AuthLayout, FormError, FormSuccess } from '../components/AuthLayout'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setSuccess(null)
    setIsLoading(true)

    try {
      const response = await forgotPasswordRequest(email)
      setSuccess(`${response.message} Check the API console for the reset link.`)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to process request.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <AuthLayout title="Forgot password">
      <form className="auth-form" onSubmit={handleSubmit}>
        <FormError message={error} />
        <FormSuccess message={success} />

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

        <button type="submit" className="primary-action" disabled={!email.trim() || isLoading}>
          {isLoading ? 'Sending...' : 'Send reset link'}
        </button>
      </form>

      <div className="auth-switch">
        <NavLink to="/login">Back to sign in</NavLink>
      </div>
    </AuthLayout>
  )
}
