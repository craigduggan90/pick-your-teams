import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'

interface PageTitleContextValue {
  title: string
  setTitle: (title: string) => void
}

const PageTitleContext = createContext<PageTitleContextValue | null>(null)

export function PageTitleProvider({
  children,
  initialTitle,
}: {
  children: ReactNode
  initialTitle: string
}) {
  const [title, setTitle] = useState(initialTitle)
  return (
    <PageTitleContext.Provider value={{ title, setTitle }}>{children}</PageTitleContext.Provider>
  )
}

export function useHeaderTitle() {
  const context = useContext(PageTitleContext)
  if (!context) {
    throw new Error('useHeaderTitle must be used within a PageTitleProvider')
  }
  return context.title
}

/**
 * Sets the shared app header's title for as long as the calling page is mounted. Every routed
 * page should call this — there's no implicit fallback once you navigate away from one page that
 * set it, so the previous title would otherwise linger.
 */
export function usePageTitle(title: string) {
  const context = useContext(PageTitleContext)
  if (!context) {
    throw new Error('usePageTitle must be used within a PageTitleProvider')
  }
  const { setTitle } = context
  useEffect(() => {
    setTitle(title)
  }, [title, setTitle])
}
