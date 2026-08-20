import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { PageTitleProvider } from '@/hooks/usePageTitle'
import { useGames } from '@/hooks/useGames'
import { GamesListPage } from './GamesListPage'
import type { GameModel } from '@/api/games'

vi.mock('@/hooks/useGames')

const baseGame: GameModel = {
  id: 'game-1',
  location: 'Oak Leaf Leisure Centre',
  startTime: '2026-08-10T20:00:00.000Z',
  duration: 60,
  teamSize: 5,
  status: 'Scheduled',
  organiser: null,
}

function renderPage(children: ReactNode = <GamesListPage />) {
  return render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <MemoryRouter>{children}</MemoryRouter>
    </PageTitleProvider>,
  )
}

describe('GamesListPage', () => {
  it('shows a loading state', () => {
    vi.mocked(useGames).mockReturnValue({
      isPending: true,
      isError: false,
      isSuccess: false,
    } as unknown as ReturnType<typeof useGames>)

    renderPage()

    expect(screen.getByText('Loading…')).toBeInTheDocument()
  })

  it('shows an error state', () => {
    vi.mocked(useGames).mockReturnValue({
      isPending: false,
      isError: true,
      isSuccess: false,
    } as unknown as ReturnType<typeof useGames>)

    renderPage()

    expect(screen.getByText('Something went wrong loading games.')).toBeInTheDocument()
  })

  it('shows an empty state', () => {
    vi.mocked(useGames).mockReturnValue({
      isPending: false,
      isError: false,
      isSuccess: true,
      data: { pages: [{ data: [], cursor: null, count: 0 }] },
      hasNextPage: false,
    } as unknown as ReturnType<typeof useGames>)

    renderPage()

    expect(screen.getByText('No Games Found!')).toBeInTheDocument()
  })

  it('renders a page of results', () => {
    vi.mocked(useGames).mockReturnValue({
      isPending: false,
      isError: false,
      isSuccess: true,
      data: { pages: [{ data: [baseGame], cursor: null, count: 1 }] },
      hasNextPage: false,
    } as unknown as ReturnType<typeof useGames>)

    renderPage()

    expect(screen.getByText('Oak Leaf Leisure Centre')).toBeInTheDocument()
  })

  it('shows Load More when there is a next page and fetches it on click', async () => {
    const fetchNextPage = vi.fn()
    vi.mocked(useGames).mockReturnValue({
      isPending: false,
      isError: false,
      isSuccess: true,
      data: { pages: [{ data: [baseGame], cursor: 'next', count: 1 }] },
      hasNextPage: true,
      isFetchingNextPage: false,
      fetchNextPage,
    } as unknown as ReturnType<typeof useGames>)
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Load More…' }))

    expect(fetchNextPage).toHaveBeenCalled()
  })

  it('opens the search panel from the Search button', async () => {
    vi.mocked(useGames).mockReturnValue({
      isPending: false,
      isError: false,
      isSuccess: true,
      data: { pages: [{ data: [], cursor: null, count: 0 }] },
      hasNextPage: false,
    } as unknown as ReturnType<typeof useGames>)
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(screen.getByText('Games / Search')).toBeInTheDocument()
  })

  it('renders New Game disabled — no create-game screen exists yet', () => {
    vi.mocked(useGames).mockReturnValue({
      isPending: false,
      isError: false,
      isSuccess: true,
      data: { pages: [{ data: [], cursor: null, count: 0 }] },
      hasNextPage: false,
    } as unknown as ReturnType<typeof useGames>)

    renderPage()

    expect(screen.getByRole('button', { name: 'New Game' })).toBeDisabled()
  })
})
