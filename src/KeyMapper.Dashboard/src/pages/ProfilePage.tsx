import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { ApiError, changePasswordRequest, getProfileRequest } from '../api/client'
import type { ProfileResponse } from '../auth/tokenStorage'
import { useAuth } from '../auth/AuthContext'
import { AppShell } from '../components/AppShell'
import { FormError, FormSuccess } from '../components/AuthLayout'

export function ProfilePage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [profile, setProfile] = useState<ProfileResponse | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [passwordError, setPasswordError] = useState<string | null>(null)
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  useEffect(() => {
    getProfileRequest()
      .then(setProfile)
      .catch((err: unknown) => {
        setLoadError(err instanceof ApiError ? err.message : 'Unable to load profile.')
      })
  }, [])

  async function handlePasswordSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPasswordError(null)
    setPasswordSuccess(null)

    if (newPassword !== confirmPassword) {
      setPasswordError('New passwords do not match.')
      return
    }

    setIsSaving(true)

    try {
      const response = await changePasswordRequest(currentPassword, newPassword)
      setPasswordSuccess(response.message)
      setCurrentPassword('')
      setNewPassword('')
      setConfirmPassword('')
      await logout()
      navigate('/login')
    } catch (err) {
      setPasswordError(err instanceof ApiError ? err.message : 'Unable to change password.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <AppShell>
      <header className="dashboard-header">
        <div>
          <h1>Profile</h1>
          <p>View your account details and update your password</p>
        </div>
      </header>

      <section className="profile-panel">
        <h2>Account</h2>
        <FormError message={loadError} />

        {profile ? (
          <dl className="profile-details">
            <div>
              <dt>Username</dt>
              <dd>{profile.username}</dd>
            </div>
            <div>
              <dt>Email</dt>
              <dd>{profile.email}</dd>
            </div>
            <div>
              <dt>Email verified</dt>
              <dd>{profile.isEmailVerified ? 'Yes' : 'No'}</dd>
            </div>
            <div>
              <dt>Member since</dt>
              <dd>{new Date(profile.createdAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        ) : (
          !loadError && <p className="auth-status">Loading profile...</p>
        )}
      </section>

      <section className="profile-panel">
        <h2>Change password</h2>
        <form className="auth-form profile-form" onSubmit={handlePasswordSubmit}>
          <FormError message={passwordError} />
          <FormSuccess message={passwordSuccess} />

          <label>
            Current password
            <input
              type="password"
              name="currentPassword"
              autoComplete="current-password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
          </label>

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
            Confirm new password
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
            disabled={!currentPassword || !newPassword || !confirmPassword || isSaving}
          >
            {isSaving ? 'Saving...' : 'Update password'}
          </button>
        </form>
      </section>
    </AppShell>
  )
}
