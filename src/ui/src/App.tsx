import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Route, Routes } from 'react-router'
import { Header } from '@/components/Header'
import { Footer } from '@/components/Footer'
import { Toaster } from '@/components/Toast'
import { ComponentsShowcasePage } from '@/pages/dev/ComponentsShowcasePage'

const queryClient = new QueryClient()

function HomePlaceholder() {
  return <p className="p-4 text-sm text-light-grey">Screens land in later stages.</p>
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Header title="Pick Your Teams" />
        <main className="flex-1">
          <Routes>
            <Route path="/" element={<HomePlaceholder />} />
            <Route path="/dev/components" element={<ComponentsShowcasePage />} />
          </Routes>
        </main>
        <Footer />
        <Toaster />
      </BrowserRouter>
    </QueryClientProvider>
  )
}
