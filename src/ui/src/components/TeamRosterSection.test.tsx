import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import { TeamRosterSection } from './TeamRosterSection'
import type { GameTeamPlayerModel } from '@/api/games'

function rowOrder(container: HTMLElement) {
  return [...container.querySelectorAll('[data-testid^="team-roster-row-"]')].map((row) =>
    row.getAttribute('data-testid')!.replace('team-roster-row-', ''),
  )
}

describe('TeamRosterSection', () => {
  it('sorts by tag (A-Z, nulls last), then display name, then rating descending', () => {
    const players: GameTeamPlayerModel[] = [
      { id: 'zed-dummy', displayName: 'Zed Dummy', tag: null, rating: 500 },
      { id: 'bob', displayName: 'Bob', tag: 'zzz', rating: 100 },
      { id: 'alice', displayName: 'Alice', tag: 'aaa', rating: 100 },
      { id: 'higher-rated', displayName: 'Higher Rated', tag: null, rating: 900 },
      { id: 'same-name-lower', displayName: 'Same Name', tag: null, rating: 300 },
      { id: 'same-name-higher', displayName: 'Same Name', tag: null, rating: 700 },
    ]

    const { container } = render(<TeamRosterSection team="None" players={players} />)

    expect(rowOrder(container)).toEqual([
      'alice',
      'bob',
      'higher-rated',
      'same-name-higher',
      'same-name-lower',
      'zed-dummy',
    ])
  })

  it('renders topContent between the heading and the player rows', () => {
    render(
      <TeamRosterSection
        team="None"
        players={[{ id: '1', displayName: 'Only Player', tag: null, rating: 500 }]}
        topContent={<button type="button">Add Player Action</button>}
      />,
    )

    const heading = screen.getByRole('heading', { name: 'Unassigned' })
    const action = screen.getByRole('button', { name: 'Add Player Action' })
    const row = screen.getByText('Only Player')

    // Heading -> topContent -> rows, in document order.
    expect(
      heading.compareDocumentPosition(action) & Node.DOCUMENT_POSITION_FOLLOWING,
    ).toBeTruthy()
    expect(action.compareDocumentPosition(row) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy()
  })
})
