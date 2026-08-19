/// <reference types="vitest/config" />
import path from 'node:path'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        '@': path.resolve(import.meta.dirname, './src'),
      },
    },
    server: {
      proxy: {
        // Local-dev-only auth shim (see docs/claude/stage-2.md) — the API reads these headers
        // instead of a bearer token, and nothing here locally plays the role of the AWS Lambda
        // authorizer that injects them in production.
        '/api': {
          target: 'http://localhost:5199',
          changeOrigin: true,
          configure: (proxy) => {
            proxy.on('proxyReq', (proxyReq) => {
              proxyReq.setHeader('Teams-User-Id', env.VITE_DEV_USER_ID ?? '')
              proxyReq.setHeader('Teams-User-Tag', env.VITE_DEV_USER_TAG ?? '')
              proxyReq.setHeader('Teams-User-Name', env.VITE_DEV_USER_NAME ?? '')
            })
          },
        },
      },
    },
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
    },
  }
})
