import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT || '3000'),
    host: true,
    proxy: {
      '/api': {
        target: process.env.services__apiservice__https__0 || process.env.services__apiservice__http__0 || 'http://localhost:5475',
        changeOrigin: true,
        secure: false
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: true,
  }
})