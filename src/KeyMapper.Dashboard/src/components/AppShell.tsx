import { Keyboard, LogIn, LogOut, User } from 'lucide-react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function AppShell({ children }: { children: React.ReactNode }) {
  const { logout, user } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="brand-row">
          <span className="brand-mark" aria-hidden="true">
            <Keyboard size={20} />
          </span>
          <span>KeyMapper</span>
        </div>

        <nav>
          <NavLink to="/dashboard">Dashboard</NavLink>
          <NavLink to="/profile">
            <User size={18} />
            Profile
          </NavLink>
          <button type="button" className="sidebar-button" onClick={handleLogout}>
            <LogOut size={18} />
            Log out
          </button>
        </nav>

        {user && <p className="sidebar-user">Signed in as {user.username}</p>}
      </aside>

      <section className="dashboard-content">{children}</section>
    </main>
  )
}

export function AuthLinkRow() {
  return (
    <p className="auth-helper">
      <NavLink to="/forgot-password">Forgot password?</NavLink>
    </p>
  )
}

export function LoginIconButton() {
  return <LogIn size={18} />
}
