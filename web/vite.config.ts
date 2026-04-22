import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  base: '/moony/',
  plugins: [react()],
  server: {
    proxy: {
      '/moony/api': {
        target: 'http://127.0.0.1:8300',
        rewrite: (path) => path.replace(/^\/moony/, '')
      }
    }
  }
})
