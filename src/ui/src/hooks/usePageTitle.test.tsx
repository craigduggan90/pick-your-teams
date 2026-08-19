import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { PageTitleProvider, useHeaderTitle, usePageTitle } from './usePageTitle'

function HeaderStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function PageA() {
  usePageTitle('Page A')
  return <p>Page A content</p>
}

function PageB() {
  usePageTitle('Page B')
  return <p>Page B content</p>
}

describe('usePageTitle', () => {
  it('exposes the initial title until a page sets its own', () => {
    render(
      <PageTitleProvider initialTitle="Pick Your Teams">
        <HeaderStub />
      </PageTitleProvider>,
    )

    expect(screen.getByRole('heading')).toHaveTextContent('Pick Your Teams')
  })

  it('updates the shared title when a page mounts', () => {
    render(
      <PageTitleProvider initialTitle="Pick Your Teams">
        <HeaderStub />
        <PageA />
      </PageTitleProvider>,
    )

    expect(screen.getByRole('heading')).toHaveTextContent('Page A')
  })

  it('reflects the most recently mounted page when navigating between them', () => {
    const { rerender } = render(
      <PageTitleProvider initialTitle="Pick Your Teams">
        <HeaderStub />
        <PageA />
      </PageTitleProvider>,
    )
    expect(screen.getByRole('heading')).toHaveTextContent('Page A')

    rerender(
      <PageTitleProvider initialTitle="Pick Your Teams">
        <HeaderStub />
        <PageB />
      </PageTitleProvider>,
    )

    expect(screen.getByRole('heading')).toHaveTextContent('Page B')
  })

  it('throws when used outside a PageTitleProvider', () => {
    function Broken() {
      usePageTitle('Nope')
      return null
    }

    expect(() => render(<Broken />)).toThrow('usePageTitle must be used within a PageTitleProvider')
  })
})
