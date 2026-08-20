import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { GameListItem } from './GameListItem'
import type { GameModel } from '@/api/games'

const baseGame: GameModel = {
  id: 'game-1',
  location: 'Oak Leaf Leisure Centre',
  startTime: '2026-08-10T20:00:00.000Z',
  duration: 60,
  teamSize: 5,
  status: 'Scheduled',
  organiser: { id: 'user-1', tag: 'little-bobby-tables', displayName: 'Robert D. Tables' },
}

function renderItem(game: GameModel) {
  return render(
    <MemoryRouter>
      <GameListItem game={game} />
    </MemoryRouter>,
  )
}

describe('GameListItem', () => {
  it('renders the formatted date, location, status, and organiser', () => {
    renderItem(baseGame)

    expect(screen.getByText('Monday 10th August @ 20:00 (UTC)')).toBeInTheDocument()
    expect(screen.getByText('Oak Leaf Leisure Centre')).toBeInTheDocument()
    expect(screen.getByText('Scheduled')).toBeInTheDocument()
    expect(screen.getByText('Organised by @little-bobby-tables')).toBeInTheDocument()
  })

  it('links to the game detail route', () => {
    renderItem(baseGame)

    expect(screen.getByRole('link')).toHaveAttribute('href', '/games/game-1')
  })

  it('falls back to placeholder text when location is null', () => {
    renderItem({ ...baseGame, location: null })

    expect(screen.getByText('Location TBC')).toBeInTheDocument()
  })

  it('omits the organiser line when organiser is null', () => {
    renderItem({ ...baseGame, organiser: null })

    expect(screen.queryByText(/Organised by/)).not.toBeInTheDocument()
  })
})
