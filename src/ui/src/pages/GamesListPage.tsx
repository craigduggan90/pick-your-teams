import { useState } from 'react'
import { Button } from '@/components/Button'
import { Loading } from '@/components/Loading'
import { ErrorMessage } from '@/components/ErrorMessage'
import { GameListItem } from '@/components/GameListItem'
import { GamesSearchPanel, type GamesSearchFilters } from '@/components/GamesSearchPanel'
import { useGames } from '@/hooks/useGames'
import { usePageTitle } from '@/hooks/usePageTitle'

export function GamesListPage() {
  usePageTitle('Games')
  const [searchOpen, setSearchOpen] = useState(false)
  const [filters, setFilters] = useState<GamesSearchFilters>({})
  const gamesQuery = useGames(filters)

  const games = gamesQuery.data?.pages.flatMap((page) => page.data) ?? []

  return (
    <div className="mx-auto flex w-full max-w-md flex-1 flex-col">
      <div className="flex-1 p-4">
        {gamesQuery.isPending && <Loading />}
        {gamesQuery.isError && (
          <ErrorMessage>Something went wrong loading games.</ErrorMessage>
        )}
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

      <div className="flex justify-between gap-2 border-t border-border p-4">
        {/* No Create Game screen exists in any stage's scope yet (see docs/claude/stage-3.md) —
            rendered per the diagram, disabled until that screen is designed and built. */}
        <Button variant="outline" disabled>
          New Game
        </Button>
        <Button
          variant="primary"
          onClick={() => setSearchOpen(true)}
          disabled={gamesQuery.isPending}
        >
          Search
        </Button>
      </div>

      <GamesSearchPanel
        open={searchOpen}
        onOpenChange={setSearchOpen}
        filters={filters}
        onApply={setFilters}
      />
    </div>
  )
}
