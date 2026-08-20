import { useState } from 'react'
import { useNavigate } from 'react-router'
import { Button } from '@/components/Button'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { GameListItem } from '@/components/GameListItem'
import { GamesSearchForm, type GamesSearchFilters } from '@/components/GamesSearchForm'
import { useGames } from '@/hooks/useGames'
import { usePageTitle } from '@/hooks/usePageTitle'
import { usePageFooterActions } from '@/hooks/usePageActions'

// A thin switch between two mutually-exclusive "pages" sharing this route — deliberately doesn't
// call usePageTitle/usePageFooterActions itself. Both branches below are components that do, and
// only one is ever mounted at a time. If this wrapper also called them, its effect would run
// after — and stomp — whichever branch is actually showing (React runs a child's effects before
// its parent's on the same commit); see docs/claude/stage-3.md.
export function GamesListPage() {
  const [searchOpen, setSearchOpen] = useState(false)
  const [filters, setFilters] = useState<GamesSearchFilters>({})

  if (searchOpen) {
    return (
      <GamesSearchForm
        filters={filters}
        onApply={(next) => {
          setFilters(next)
          setSearchOpen(false)
        }}
        onCancel={() => setSearchOpen(false)}
      />
    )
  }

  return <GamesListContent filters={filters} onSearch={() => setSearchOpen(true)} />
}

function GamesListContent({
  filters,
  onSearch,
}: {
  filters: GamesSearchFilters
  onSearch: () => void
}) {
  usePageTitle('Games')
  const navigate = useNavigate()
  const gamesQuery = useGames(filters)
  const games = gamesQuery.data?.pages.flatMap((page) => page.data) ?? []

  usePageFooterActions(
    <div className="flex w-full justify-between gap-2 p-4">
      <Button variant="outline" onClick={() => navigate('/games/new')}>
        New Game
      </Button>
      <Button variant="primary" onClick={onSearch} disabled={gamesQuery.isPending}>
        Search
      </Button>
    </div>,
  )

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col">
      <div className="flex-1 p-4">
        {gamesQuery.isPending && <Loading />}
        {gamesQuery.isError && <ErrorMessage>Something went wrong loading games.</ErrorMessage>}
        {gamesQuery.isSuccess && games.length === 0 && (
          <p className="p-4 text-center text-sm text-light-grey">No Games Found!</p>
        )}
        {gamesQuery.isSuccess && games.length > 0 && (
          <div className="flex flex-col gap-3">
            {games.map((game) => (
              <GameListItem key={game.id} game={game} />
            ))}
            {gamesQuery.hasNextPage && (
              <Button
                variant="outline"
                className="w-full"
                onClick={() => gamesQuery.fetchNextPage()}
                disabled={gamesQuery.isFetchingNextPage}
              >
                {gamesQuery.isFetchingNextPage ? 'Loading…' : 'Load More…'}
              </Button>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
