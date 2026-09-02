import { useEffect, useState, type ReactNode } from 'react'
import { Activity, Database, Keyboard, MonitorCheck } from 'lucide-react'
import { AppShell } from '../components/AppShell'

export function DashboardPage() {
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
    <AppShell>
      <header className="dashboard-header">
        <div>
          <h1>Dashboard</h1>
          <p>Phase 1 workspace — authentication is active</p>
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
        <p className="panel-note">Key statistics will appear here in Phase 4.</p>
        <div className="empty-chart" aria-hidden="true">
          <span />
          <span />
          <span />
          <span />
          <span />
          <span />
        </div>
      </section>
    </AppShell>
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
