import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router'
import { useSelf } from '@/hooks/useSelf'
import { TagGate } from './TagGate'

vi.mock('@/hooks/useSelf')

function renderGate() {
  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route
          path="/protected"
          element={
            <TagGate>
              <p>Protected content</p>
            </TagGate>
          }
        />
        <Route path="/tag-setup" element={<p>Tag setup screen</p>} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('TagGate', () => {
  it('shows a loading state while the self query is pending', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderGate()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state if the self query fails', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: true,
    } as unknown as ReturnType<typeof useSelf>)

    renderGate()

    expect(screen.getByText('Something went wrong loading your account.')).toBeInTheDocument()
  })

  it('redirects to /tag-setup when the user still needs to set a tag', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: '1', tag: '1' },
    } as unknown as ReturnType<typeof useSelf>)

    renderGate()

    expect(screen.getByText('Tag setup screen')).toBeInTheDocument()
  })

  it('renders children once the user has a tag set', () => {
    vi.mocked(useSelf).mockReturnValue({
      isPending: false,
      isError: false,
      data: { id: '1', tag: 'bob' },
    } as unknown as ReturnType<typeof useSelf>)

    renderGate()

    expect(screen.getByText('Protected content')).toBeInTheDocument()
  })
})
