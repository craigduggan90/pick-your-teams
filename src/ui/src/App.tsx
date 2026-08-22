import type { ReactNode } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes, useNavigate } from 'react-router'
import { Auth0Provider, type AppState } from '@auth0/auth0-react'
import { Header } from '@/components/Header'
import { Footer } from '@/components/Footer'
import { Toaster } from '@/components/Toast'
import { RequireAuth } from '@/components/RequireAuth'
import { RequireAuthAndTag } from '@/components/RequireAuthAndTag'
import { PageTitleProvider, useHeaderTitle } from '@/hooks/usePageTitle'
import { PageActionsProvider, useFooterActions } from '@/hooks/usePageActions'
import { APP_NAME } from '@/lib/constants'
import { ComponentsShowcasePage } from '@/pages/dev/ComponentsShowcasePage'
import { TeamPickerPage } from '@/pages/TeamPickerPage'
import { ChangeTagPage } from '@/pages/ChangeTagPage'
import { MyAccountPage } from '@/pages/MyAccountPage'
import { GameViewPage } from '@/pages/GameViewPage'
import { NewGamePage } from '@/pages/NewGamePage'
import { GameTeamsPage } from '@/pages/GameTeamsPage'
import { InvitePlayersPage } from '@/pages/InvitePlayersPage'
import { MyInvitationsPage } from '@/pages/MyInvitationsPage'

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
  const footerActions = useFooterActions()

  return (
    <>
      <Header title={title} />
      <main className="min-h-0 flex-1 overflow-y-auto">{children}</main>
      <Footer actions={footerActions} />
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
            <PageActionsProvider>
              <AppShell>
                <Routes>
                  <Route path="/" element={<TeamPickerPage />} />
                  <Route
                    path="/change-tag"
                    element={
                      <RequireAuth>
                        <ChangeTagPage />
                      </RequireAuth>
                    }
                  />
                  <Route
                    path="/account"
                    element={
                      <RequireAuthAndTag>
                        <MyAccountPage />
                      </RequireAuthAndTag>
                    }
                  />
                  <Route
                    path="/games/new"
                    element={
                      <RequireAuthAndTag>
                        <NewGamePage />
                      </RequireAuthAndTag>
                    }
                  />
                  <Route
                    path="/games/:id"
                    element={
                      <RequireAuthAndTag>
                        <GameViewPage />
                      </RequireAuthAndTag>
                    }
                  />
                  <Route
                    path="/games/:id/teams"
                    element={
                      <RequireAuthAndTag>
                        <GameTeamsPage />
                      </RequireAuthAndTag>
                    }
                  />
                  <Route
                    path="/games/:id/invite"
                    element={
                      <RequireAuthAndTag>
                        <InvitePlayersPage />
                      </RequireAuthAndTag>
                    }
                  />
                  <Route
                    path="/invitations"
                    element={
                      <RequireAuthAndTag>
                        <MyInvitationsPage />
                      </RequireAuthAndTag>
                    }
                  />
                  <Route path="/dev/components" element={<ComponentsShowcasePage />} />
                </Routes>
              </AppShell>
            </PageActionsProvider>
          </PageTitleProvider>
        </Auth0ProviderWithNavigate>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
