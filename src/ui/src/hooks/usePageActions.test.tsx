import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { PageActionsProvider, useFooterActions, usePageFooterActions } from './usePageActions'

function FooterProbe() {
  const actions = useFooterActions()
  return <div data-testid="footer-probe">{actions}</div>
}

function PageWithActions({ label }: { label: string }) {
  usePageFooterActions(<button type="button">{label}</button>)
  return <p>Page content</p>
}

function PageWithoutActions() {
  return <p>Other page content</p>
}

describe('usePageActions', () => {
  it('has no footer actions by default', () => {
    render(
      <PageActionsProvider>
        <FooterProbe />
      </PageActionsProvider>,
    )

    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('plants the given content in the footer actions slot', () => {
    render(
      <PageActionsProvider>
        <FooterProbe />
        <PageWithActions label="Search" />
      </PageActionsProvider>,
    )

    expect(screen.getByRole('button', { name: 'Search' })).toBeInTheDocument()
  })

  it('clears the footer actions when the page unmounts', () => {
    const { rerender } = render(
      <PageActionsProvider>
        <FooterProbe />
        <PageWithActions label="Search" />
      </PageActionsProvider>,
    )
    expect(screen.getByRole('button', { name: 'Search' })).toBeInTheDocument()

    rerender(
      <PageActionsProvider>
        <FooterProbe />
        <PageWithoutActions />
      </PageActionsProvider>,
    )

    expect(screen.queryByRole('button')).not.toBeInTheDocument()
    expect(screen.getByText('Other page content')).toBeInTheDocument()
  })
})
