import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import './index.css'
import SimulatorPanel from './SimulatorPanel.jsx'

const queryClient = new QueryClient();

createRoot(document.getElementById('root')).render(
  <StrictMode>
        <QueryClientProvider client={queryClient}>
            <SimulatorPanel />
    </QueryClientProvider>
  </StrictMode>,
)
