import type { ReactNode } from 'react'
import { useAuth0 } from '@auth0/auth0-react'
import { useNavigate } from 'react-router'

export interface HeaderProps {
  title: string
}

function HeaderIconButton({
  label,
  onClick,
  children,
}: {
  label: string
  onClick: () => void
  children?: ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={label}
      className="group flex size-8 shrink-0 cursor-pointer items-center justify-center"
    >
      {children}
    </button>
  )
}

const homeIcon = <img src="/icon-192.png" alt="" data-testid="home-icon" className="size-8 rounded-full" />
const accountIcon = (
  <img src="/account-settings.png" alt="" data-testid="account-icon" className="size-8 rounded-full" />
)
// TEMPORARY: hover-swap is just for previewing the pending-icon color, not final behavior —
// the real trigger will be the pendingInvitations count from GET /users/self, not :hover.
const invitationsIcon = (
  <span className="relative block size-8">
    <img
      src="/invitations.png"
      alt=""
      data-testid="invitations-icon"
      className="absolute inset-0 size-8 transition-opacity duration-150 group-hover:opacity-0"
    />
    <img
      src="/invitations-pending.png"
      alt=""
      data-testid="invitations-pending-icon"
      className="absolute inset-0 size-8 opacity-0 transition-opacity duration-150 group-hover:opacity-100"
    />
  </span>
)

export function Header({ title }: HeaderProps) {
  const navigate = useNavigate()
  const { isAuthenticated } = useAuth0()

  return (
    <header className="flex shrink-0 items-center gap-3 bg-primary px-4 py-3 text-primary-foreground">
      {isAuthenticated ? (
        <HeaderIconButton label="Home" onClick={() => navigate('/')}>
          {homeIcon}
        </HeaderIconButton>
      ) : (
        <div className="flex size-8 shrink-0 items-center justify-center">{homeIcon}</div>
      )}
      <h1 className="flex-1 truncate text-lg font-medium">{title}</h1>
      {isAuthenticated && (
        <HeaderIconButton label="My Invitations" onClick={() => navigate('/invitations')}>
          {invitationsIcon}
        </HeaderIconButton>
      )}
      {isAuthenticated && (
        <HeaderIconButton label="My Account" onClick={() => navigate('/account')}>
          {accountIcon}
        </HeaderIconButton>
      )}
    </header>
  )
}
