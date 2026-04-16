import path from "path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 61045,
    proxy: {
        "/auth": {
            target: "http://localhost:5046",
            changeOrigin: true,
        },
        "/api": {
            target: "http://localhost:5046",
            changeOrigin: true,
        },
      },
  },
});