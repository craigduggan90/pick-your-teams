import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

// Split into two contexts deliberately: `setFooterActions` (the dispatch context) never changes
// reference across renders, so writers (usePageFooterActions, called with a fresh JSX element on
// every render of the calling page) never re-render just because they wrote to it. Bundling state
// + setter into one context value — the usePageTitle pattern this was modeled on — works there
// because title is a stable string primitive, but ReactNode is a fresh object every render, so
// the combined form caused a real infinite loop: write → provider re-renders → context value
// object changes identity → every consumer (including the writer) re-renders → new JSX element →
// effect deps differ → write again, forever.
const PageActionsStateContext = createContext<ReactNode>(undefined)
const PageActionsDispatchContext = createContext<((node: ReactNode) => void) | null>(null)

export function PageActionsProvider({ children }: { children: ReactNode }) {
  const [footerActions, setFooterActions] = useState<ReactNode>(null)
  return (
    <PageActionsDispatchContext.Provider value={setFooterActions}>
      <PageActionsStateContext.Provider value={footerActions}>
        {children}
      </PageActionsStateContext.Provider>
    </PageActionsDispatchContext.Provider>
  )
}

export function useFooterActions() {
  // Presence-check against the dispatch context, not the state context: `footerActions` is a
  // ReactNode and can legitimately BE undefined (e.g. a page passes `game && <div>...</div>` and
  // game is falsy) — using that as the "no provider" sentinel misfires on a real, valid value.
  // The dispatch function is never falsy once a Provider exists, so it's a safe presence check.
  const setFooterActions = useContext(PageActionsDispatchContext)
  const footerActions = useContext(PageActionsStateContext)
  if (!setFooterActions) {
    throw new Error('useFooterActions must be used within a PageActionsProvider')
  }
  return footerActions
}

/**
 * Plants content in the shared app Footer's action-bar slot for as long as the calling page is
 * mounted — the pattern for any screen with buttons "stuck to the bottom" in the diagrams (Save/
 * Back, New Game/Search, etc). Unlike usePageTitle, this clears itself on unmount: most pages
 * don't have footer actions, so navigating away should silently fall back to no bar rather than
 * requiring every page to remember to call this with `null`.
 */
export function usePageFooterActions(node: ReactNode) {
  const setFooterActions = useContext(PageActionsDispatchContext)
  if (!setFooterActions) {
    throw new Error('usePageFooterActions must be used within a PageActionsProvider')
  }
  useEffect(() => {
    setFooterActions(node)
  }, [node, setFooterActions])
  // Separate effect, deliberately not depending on `node` (a fresh element every render): only
  // fires on actual unmount, not on every re-render, which would otherwise clear-then-reset the
  // footer on every commit. `setFooterActions` is a useState setter, stable across renders.
  useEffect(() => {
    return () => setFooterActions(null)
  }, [setFooterActions])
}
