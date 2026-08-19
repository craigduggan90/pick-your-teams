import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import { useUpdateTag } from '@/hooks/useUpdateTag'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { TagSetupPage } from './TagSetupPage'

vi.mock('@/hooks/useSelf')
vi.mock('@/hooks/useUpdateTag')

function renderPage() {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <MemoryRouter>
        <TagSetupPage />
      </MemoryRouter>
    </PageTitleProvider>,
  )
}

describe('TagSetupPage', () => {
  it('shows a loading state while self is resolving', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state if self fails to load', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderPage()

    expect(screen.getByText('Something went wrong loading your account.')).toBeInTheDocument()
  })

  it('renders gate mode with no Back button when the user still needs to set a tag', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'user-1' },
    } as unknown as ReturnType<typeof useSelf>)
    vi.mocked(useUpdateTag).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      isSuccess: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof useUpdateTag>)

    renderPage()

    expect(screen.getByLabelText('Tag')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Back' })).not.toBeInTheDocument()
  })

  it('renders normal mode, prefilled, with a Back button for a user who already has a tag', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: 'user-1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)
    vi.mocked(useUpdateTag).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      isSuccess: false,
      isError: false,
      error: null,
    } as unknown as ReturnType<typeof useUpdateTag>)

    renderPage()

    expect(screen.getByLabelText('Tag')).toHaveValue('bob')
    expect(screen.getByRole('button', { name: 'Back' })).toBeInTheDocument()
  })
})
