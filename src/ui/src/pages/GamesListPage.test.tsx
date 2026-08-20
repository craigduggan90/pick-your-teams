import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { useGames } from '@/hooks/useGames'
import { GamesListPage } from './GamesListPage'
import type { GameModel } from '@/api/games'

vi.mock('@/hooks/useGames')

function HeaderTitleStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

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
      <PageActionsProvider>
        <HeaderTitleStub />
        <MemoryRouter initialEntries={['/']}>
          <Routes>
            <Route path="/" element={children} />
            <Route path="/games/new" element={<p>New game screen</p>} />
          </Routes>
        </MemoryRouter>
        <FooterActionsStub />
      </PageActionsProvider>
    </PageTitleProvider>,
  )
}

function mockEmptyGames() {
  vi.mocked(useGames).mockReturnValue({
    isPending: false,
    isError: false,
    isSuccess: true,
    data: { pages: [{ data: [], cursor: null, count: 0 }] },
    hasNextPage: false,
  } as unknown as ReturnType<typeof useGames>)
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
    mockEmptyGames()

    renderPage()

    expect(screen.getByText('No Games Found!')).toBeInTheDocument()
  })

  it('sets the header title to Games', () => {
    mockEmptyGames()

    renderPage()

    expect(screen.getByRole('heading')).toHaveTextContent('Games')
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

  it('navigates to /games/new via New Game', async () => {
    mockEmptyGames()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'New Game' }))

    expect(screen.getByText('New game screen')).toBeInTheDocument()
  })

  it('replaces the list with the search form on Search, and the header title updates', async () => {
    mockEmptyGames()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(screen.getByRole('heading')).toHaveTextContent('Games / Search')
    expect(screen.getByLabelText('Players per Team')).toBeInTheDocument()
    expect(screen.queryByText('No Games Found!')).not.toBeInTheDocument()
  })

  it('returns to the list on Cancel, restoring the Games title', async () => {
    mockEmptyGames()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Search' }))
    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(screen.getByRole('heading')).toHaveTextContent('Games')
    expect(screen.getByText('No Games Found!')).toBeInTheDocument()
  })

  it('returns to the list on Apply, passing the new filters to useGames', async () => {
    mockEmptyGames()
    const user = userEvent.setup()

    renderPage()
    await user.click(screen.getByRole('button', { name: 'Search' }))
    await user.click(screen.getByRole('button', { name: 'Scheduled' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(screen.getByRole('heading')).toHaveTextContent('Games')
    expect(useGames).toHaveBeenLastCalledWith(expect.objectContaining({ status: 'Scheduled' }))
  })
})
