import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { GamesSearchForm, type GamesSearchFilters } from './GamesSearchForm'

function HeaderTitleStub() {
  return <h1>{useHeaderTitle()}</h1>
}

function FooterActionsStub() {
  return <>{useFooterActions()}</>
}

function renderForm({
  filters = {} as GamesSearchFilters,
  onApply = vi.fn(),
  onCancel = vi.fn(),
} = {}) {
  render(
    <PageTitleProvider initialTitle="Pick Your Teams">
      <PageActionsProvider>
        <HeaderTitleStub />
        <GamesSearchForm filters={filters} onApply={onApply} onCancel={onCancel} />
        <FooterActionsStub />
      </PageActionsProvider>
    </PageTitleProvider>,
  )
  return { onApply, onCancel }
}

describe('GamesSearchForm', () => {
  it('sets the header title to Games / Search', () => {
    renderForm()
    expect(screen.getByRole('heading')).toHaveTextContent('Games / Search')
  })

  it('calls onCancel via the footer Cancel button', async () => {
    const user = userEvent.setup()
    const { onCancel } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(onCancel).toHaveBeenCalled()
  })

  it('applies the selected status filter', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Scheduled' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ status: 'Scheduled' }))
  })

  it('deselects a status filter when clicked again', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.click(screen.getByRole('button', { name: 'Complete' }))
    await user.click(screen.getByRole('button', { name: 'Complete' }))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ status: undefined }))
  })

  it('includes team size only when a value is entered', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm()

    await user.type(screen.getByLabelText('Players per Team'), '5')
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ teamSize: 5 }))
  })

  it('pre-selects "Games I\'ve Organised" when it was last applied, and re-applies it', async () => {
    const user = userEvent.setup()
    const { onApply } = renderForm({ filters: { organiserOnly: true } })

    expect(screen.getByRole('button', { name: "Games I've Organised" })).toHaveAttribute(
      'aria-pressed',
      'true',
    )

    await user.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(expect.objectContaining({ organiserOnly: true }))
  })

  describe('Game Start From/To defaults', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-08-20T14:23:00.000Z'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('defaults to today through fourteen days out, always visible as plain dates', () => {
      const { onApply } = renderForm()

      expect(screen.getByLabelText('Game Start From')).toHaveValue('2026-08-20')
      expect(screen.getByLabelText('Game Start To')).toHaveValue('2026-09-03')

      fireEvent.click(screen.getByRole('button', { name: 'Apply' }))

      // Emitted as picked (inclusive) dates — the exclusive-upper-bound +1 day roll-forward for
      // the actual request happens where filters become query params (GamesListPage), not here,
      // so the persisted state re-opens the form showing what was actually picked.
      expect(onApply).toHaveBeenCalledWith(
        expect.objectContaining({
          startTimeFrom: '2026-08-20T00:00:00.000Z',
          startTimeTo: '2026-09-03T00:00:00.000Z',
        }),
      )
    })

    it('applies an edited Game Start From value', () => {
      const { onApply } = renderForm()

      fireEvent.change(screen.getByLabelText('Game Start From'), {
        target: { value: '2026-08-22' },
      })
      fireEvent.click(screen.getByRole('button', { name: 'Apply' }))

      expect(onApply).toHaveBeenCalledWith(
        expect.objectContaining({ startTimeFrom: '2026-08-22T00:00:00.000Z' }),
      )
    })
  })
})
