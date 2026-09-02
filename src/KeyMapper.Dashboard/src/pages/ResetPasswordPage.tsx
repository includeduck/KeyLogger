import { useState, type FormEvent } from 'react'
import { NavLink, useSearchParams } from 'react-router-dom'
import { ApiError, resetPasswordRequest } from '../api/client'
import { AuthLayout, FormError, FormSuccess } from '../components/AuthLayout'

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setSuccess(null)

    if (!token) {
      setError('Reset token is missing from the URL.')
      return
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setIsLoading(true)

    try {
      const response = await resetPasswordRequest(token, newPassword)
      setSuccess(response.message)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Unable to reset password.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <AuthLayout title="Reset password">
      <form className="auth-form" onSubmit={handleSubmit}>
        <FormError message={error} />
        <FormSuccess message={success} />

        <label>
          New password
          <input
            type="password"
            name="newPassword"
            autoComplete="new-password"
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
          />
        </label>

        <label>
          Confirm password
          <input
            type="password"
            name="confirmPassword"
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
          />
        </label>

        <button
          type="submit"
          className="primary-action"
          disabled={!newPassword || !confirmPassword || isLoading}
        >
          {isLoading ? 'Resetting...' : 'Reset password'}
        </button>
      </form>

      <div className="auth-switch">
        <NavLink to="/login">Back to sign in</NavLink>
      </div>
    </AuthLayout>
  )
}
