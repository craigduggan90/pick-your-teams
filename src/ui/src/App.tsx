import type { ReactNode } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes, useNavigate } from 'react-router'
import { Auth0Provider, type AppState } from '@auth0/auth0-react'
import { Header } from '@/components/Header'
import { Footer } from '@/components/Footer'
import { Toaster } from '@/components/Toast'
import { RequireAuth } from '@/components/RequireAuth'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { APP_NAME } from '@/lib/constants'
import { ComponentsShowcasePage } from '@/pages/dev/ComponentsShowcasePage'
import { TeamPickerPage } from '@/pages/TeamPickerPage'
import { TagSetupPage } from '@/pages/TagSetupPage'

const queryClient = new QueryClient()

function Auth0ProviderWithNavigate({ children }: { children: ReactNode }) {
  const navigate = useNavigate()

  const onRedirectCallback = (appState?: AppState) => {
    navigate(appState?.returnTo ?? window.location.pathname)
  }

  return (
    <Auth0Provider
      domain={import.meta.env.VITE_AUTH0_DOMAIN}
      clientId={import.meta.env.VITE_AUTH0_CLIENT_ID}
      authorizationParams={{
        redirect_uri: window.location.origin,
        audience: import.meta.env.VITE_AUTH0_AUDIENCE,
      }}
      cacheLocation="memory"
      onRedirectCallback={onRedirectCallback}
    >
      {children}
    </Auth0Provider>
  )
}

function AppShell({ children }: { children: ReactNode }) {
  const title = useHeaderTitle()

  return (
    <>
      <Header title={title} />
      <main className="flex-1">{children}</main>
      <Footer />
      <Toaster />
    </>
  )
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Auth0ProviderWithNavigate>
          <PageTitleProvider initialTitle={APP_NAME}>
            <AppShell>
              <Routes>
                <Route path="/" element={<TeamPickerPage />} />
                <Route
                  path="/tag-setup"
                  element={
                    <RequireAuth>
                      <TagSetupPage />
                    </RequireAuth>
                  }
                />
                <Route path="/dev/components" element={<ComponentsShowcasePage />} />
              </Routes>
            </AppShell>
          </PageTitleProvider>
        </Auth0ProviderWithNavigate>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
