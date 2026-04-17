import { createRoot } from 'react-dom/client'
import './index.css'
import App from './app/App.tsx'
import { AuthProvider } from './features/auth/shared/auth-context.tsx'

createRoot(document.getElementById('root')!).render(
    <AuthProvider>
        <App />
    </AuthProvider>
)
