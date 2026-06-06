import { useEffect, useState, type ReactNode } from 'react'
import { BrowserRouter, Navigate, NavLink, Route, Routes } from 'react-router-dom'
import {
  Activity,
  BarChart3,
  Database,
  Keyboard,
  LogIn,
  MonitorCheck,
  UserPlus,
} from 'lucide-react'
import './App.css'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<AuthPage mode="login" />} />
        <Route path="/register" element={<AuthPage mode="register" />} />
        <Route path="/dashboard" element={<DashboardPage />} />
      </Routes>
    </BrowserRouter>
  )
}

type AuthMode = 'login' | 'register'

function AuthPage({ mode }: { mode: AuthMode }) {
  const isLogin = mode === 'login'

  return (
    <main className="auth-layout">
      <section className="auth-panel" aria-labelledby="auth-title">
        <div className="brand-row">
          <span className="brand-mark" aria-hidden="true">
            <Keyboard size={22} />
          </span>
          <span>KeyMapper</span>
        </div>

        <h1 id="auth-title">{isLogin ? 'Sign in' : 'Create account'}</h1>

        <form className="auth-form">
          {!isLogin && (
            <label>
              Username
              <input type="text" name="username" autoComplete="username" />
            </label>
          )}

          <label>
            Email
            <input type="email" name="email" autoComplete="email" />
          </label>

          <label>
            Password
            <input
              type="password"
              name="password"
              autoComplete={isLogin ? 'current-password' : 'new-password'}
            />
          </label>

          {!isLogin && (
            <label className="terms-row">
              <input type="checkbox" name="terms" />
              <span>I accept the terms of service</span>
            </label>
          )}

          <button type="button" className="primary-action">
            {isLogin ? <LogIn size={18} /> : <UserPlus size={18} />}
            {isLogin ? 'Sign in' : 'Register'}
          </button>
        </form>

        <div className="auth-switch">
          {isLogin ? (
            <NavLink to="/register">Create account</NavLink>
          ) : (
            <NavLink to="/login">Back to sign in</NavLink>
          )}
        </div>
      </section>
    </main>
  )
}

function DashboardPage() {
  const [apiStatus, setApiStatus] = useState<'Checking' | 'Healthy' | 'Unavailable'>('Checking')

  useEffect(() => {
    let isMounted = true

    fetch('/api/health')
      .then((response) => {
        if (!response.ok) {
          throw new Error('Health check failed')
        }

        return response.json()
      })
      .then((data: { status?: string }) => {
        if (isMounted) {
          setApiStatus(data.status === 'Healthy' ? 'Healthy' : 'Unavailable')
        }
      })
      .catch(() => {
        if (isMounted) {
          setApiStatus('Unavailable')
        }
      })

    return () => {
      isMounted = false
    }
  }, [])

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
          <NavLink to="/dashboard">
            <BarChart3 size={18} />
            Dashboard
          </NavLink>
          <NavLink to="/login">
            <LogIn size={18} />
            Sign in
          </NavLink>
        </nav>
      </aside>

      <section className="dashboard-content" aria-labelledby="dashboard-title">
        <header className="dashboard-header">
          <div>
            <h1 id="dashboard-title">Dashboard</h1>
            <p>Phase 0 workspace</p>
          </div>
          <span className={`status-pill ${apiStatus.toLowerCase()}`} aria-live="polite">
            API {apiStatus}
          </span>
        </header>

        <section className="metric-grid" aria-label="Keyboard metrics">
          <MetricCard icon={<Activity size={20} />} label="Today" value="0" />
          <MetricCard icon={<Keyboard size={20} />} label="Lifetime" value="0" />
          <MetricCard icon={<MonitorCheck size={20} />} label="Devices" value="0" />
          <MetricCard icon={<Database size={20} />} label="Pending sync" value="0" />
        </section>

        <section className="placeholder-panel">
          <h2>Statistics</h2>
          <div className="empty-chart" aria-hidden="true">
            <span />
            <span />
            <span />
            <span />
            <span />
            <span />
          </div>
        </section>
      </section>
    </main>
  )
}

function MetricCard({
  icon,
  label,
  value,
}: {
  icon: ReactNode
  label: string
  value: string
}) {
  return (
    <article className="metric-card">
      <div className="metric-icon">{icon}</div>
      <div>
        <p>{label}</p>
        <strong>{value}</strong>
      </div>
    </article>
  )
}

export default App
