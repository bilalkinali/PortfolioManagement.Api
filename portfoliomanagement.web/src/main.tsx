import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router'
import './index.css'
import { router } from './app/router.tsx'
import { AuthProvider } from './features/auth/shared/auth-context.tsx'

const savedTheme = window.localStorage.getItem('theme')
const initialTheme = savedTheme === 'light' ? 'light' : 'dark'

document.documentElement.classList.toggle('dark', initialTheme === 'dark')
document.documentElement.style.colorScheme = initialTheme
document.body.classList.add('app-ready')

createRoot(document.getElementById('root')!).render(
    <AuthProvider>
        <RouterProvider router={router} />
    </AuthProvider>
)
