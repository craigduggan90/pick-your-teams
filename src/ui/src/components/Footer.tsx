import type { ReactNode } from 'react'
import { APP_NAME } from '@/lib/constants'

export interface FooterProps {
  /** Page-supplied bottom bar content, planted via usePageFooterActions — see 02-games-list.png's
   * "Maybe tiny footer text that has the app name in it" note, pointing at the same bottom bar as
   * the New Game/Search buttons. */
  actions?: ReactNode
}

export function Footer({ actions }: FooterProps) {
  if (actions) {
    return (
      <footer className="mt-auto flex flex-col">
        <div className="mx-auto flex w-full max-w-md justify-center border-t border-border">
          {actions}
        </div>
      </footer>
    )
  }

  return (
    <footer className="mt-auto flex flex-col">
      <p className="px-4 py-3 text-center text-xs text-light-grey">{APP_NAME}</p>
    </footer>
  )
}
