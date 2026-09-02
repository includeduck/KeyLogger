import { Keyboard } from 'lucide-react'
import { NavLink } from 'react-router-dom'

export function AuthLayout({
  title,
  children,
}: {
  title: string
  children: React.ReactNode
}) {
  return (
    <main className="auth-layout">
      <section className="auth-panel" aria-labelledby="auth-title">
        <div className="brand-row">
          <span className="brand-mark" aria-hidden="true">
            <Keyboard size={22} />
          </span>
          <span>KeyMapper</span>
        </div>

        <h1 id="auth-title">{title}</h1>
        {children}
      </section>
    </main>
  )
}

export function AuthSwitch({ isLogin }: { isLogin: boolean }) {
  return (
    <div className="auth-switch">
      {isLogin ? <NavLink to="/register">Create account</NavLink> : <NavLink to="/login">Back to sign in</NavLink>}
    </div>
  )
}

export function FormError({ message }: { message: string | null }) {
  if (!message) {
    return null
  }

  return (
    <p className="form-error" role="alert">
      {message}
    </p>
  )
}

export function FormSuccess({ message }: { message: string | null }) {
  if (!message) {
    return null
  }

  return (
    <p className="form-success" role="status">
      {message}
    </p>
  )
}
