import { useEffect, useState } from 'react'
import { NavLink, useSearchParams } from 'react-router-dom'
import { ApiError, verifyEmailRequest } from '../api/client'
import { AuthLayout, FormError, FormSuccess } from '../components/AuthLayout'

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    let isMounted = true

    async function verify() {
      if (!token) {
        if (isMounted) {
          setError('Verification token is missing from the URL.')
          setIsLoading(false)
        }
        return
      }

      try {
        const response = await verifyEmailRequest(token)
        if (isMounted) {
          setSuccess(response.message)
        }
      } catch (err) {
        if (isMounted) {
          setError(err instanceof ApiError ? err.message : 'Unable to verify email.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    verify()

    return () => {
      isMounted = false
    }
  }, [token])

  return (
    <AuthLayout title="Verify email">
      {isLoading ? <p className="auth-status">Verifying your email...</p> : null}
      <FormError message={error} />
      <FormSuccess message={success} />

      <div className="auth-switch">
        <NavLink to="/login">Go to sign in</NavLink>
      </div>
    </AuthLayout>
  )
}
