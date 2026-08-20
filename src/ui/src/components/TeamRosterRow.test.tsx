import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TeamRosterRow } from './TeamRosterRow'
import type { GameTeamPlayerModel } from '@/api/games'

const player: GameTeamPlayerModel = {
  id: 'p-1',
  displayName: 'Jess B',
  tag: 'jessb',
  rating: 1000,
}

describe('TeamRosterRow', () => {
  it('renders the display name, tag, and rating', () => {
    render(<TeamRosterRow player={player} team="Home" />)

    expect(screen.getByText('Jess B')).toBeInTheDocument()
    expect(screen.getByText('(@jessb)')).toBeInTheDocument()
    expect(screen.getByText('Rating 1000')).toBeInTheDocument()
    expect(screen.getByText('Home')).toBeInTheDocument()
  })

  it('renders no select when not editable', () => {
    render(<TeamRosterRow player={player} team="Home" />)

    expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
  })

  it('offers Away/Remove-from-Team/Remove-from-Game for a Home player, never Home itself', async () => {
    const user = userEvent.setup()
    render(<TeamRosterRow player={player} team="Home" editable />)

    await user.click(screen.getByRole('combobox'))

    expect(await screen.findByRole('option', { name: 'To Away Team' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Remove from Team' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Remove from Game' })).toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'To Home Team' })).not.toBeInTheDocument()
  })

  it('offers Home/Away/Remove-from-Game for an unassigned player, no Remove-from-Team', async () => {
    const user = userEvent.setup()
    render(<TeamRosterRow player={player} team="None" editable />)

    await user.click(screen.getByRole('combobox'))

    expect(await screen.findByRole('option', { name: 'To Home Team' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'To Away Team' })).toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'Remove from Team' })).not.toBeInTheDocument()
  })

  it('calls onTeamChange for a team move', async () => {
    const user = userEvent.setup()
    const onTeamChange = vi.fn()
    render(<TeamRosterRow player={player} team="Home" editable onTeamChange={onTeamChange} />)

    await user.click(screen.getByRole('combobox'))
    await user.click(await screen.findByRole('option', { name: 'To Away Team' }))

    expect(onTeamChange).toHaveBeenCalledWith('p-1', 'Away')
  })

  it('calls onRemove, not onTeamChange, for Remove from Game', async () => {
    const user = userEvent.setup()
    const onTeamChange = vi.fn()
    const onRemove = vi.fn()
    render(
      <TeamRosterRow
        player={player}
        team="Home"
        editable
        onTeamChange={onTeamChange}
        onRemove={onRemove}
      />,
    )

    await user.click(screen.getByRole('combobox'))
    await user.click(await screen.findByRole('option', { name: 'Remove from Game' }))

    expect(onRemove).toHaveBeenCalledWith(player)
    expect(onTeamChange).not.toHaveBeenCalled()
  })
})
