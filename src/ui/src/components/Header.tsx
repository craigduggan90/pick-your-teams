import { useAuth0 } from '@auth0/auth0-react'
import { useNavigate } from 'react-router'

export interface HeaderProps {
  title: string
}

function HeaderIconButton({
  label,
  onClick,
  disabled,
}: {
  label: string
  onClick: () => void
  disabled?: boolean
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-label={label}
      className="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary-foreground/20 transition-colors hover:bg-primary-foreground/30 disabled:pointer-events-none disabled:opacity-40"
    >
      <span aria-hidden="true" className="size-4 rounded-full bg-primary-foreground/60" />
    </button>
  )
}

export function Header({ title }: HeaderProps) {
  const navigate = useNavigate()
  const { isAuthenticated } = useAuth0()

  return (
    <header className="flex items-center gap-3 bg-primary px-4 py-3 text-primary-foreground">
      <HeaderIconButton label="Home" onClick={() => navigate('/')} disabled={!isAuthenticated} />
      <h1 className="flex-1 truncate text-lg font-medium">{title}</h1>
      {isAuthenticated && (
        <HeaderIconButton label="My Account" onClick={() => navigate('/account')} />
      )}
    </header>
  )
}
