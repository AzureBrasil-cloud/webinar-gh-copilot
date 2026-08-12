import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import tailwindcss from "@tailwindcss/vite";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  build: {
    outDir: path.resolve(__dirname, "../wwwroot/dist"),
    emptyOutDir: true,
    rollupOptions: {
      input: path.resolve(__dirname, "src/main.ts"),
      output: {
        entryFileNames: "main.js",
        assetFileNames: (assetInfo) =>
          assetInfo.names?.[0]?.endsWith(".css") ? "main.css" : "[name][extname]",
        chunkFileNames: "chunks/[name].js",
      },
    },
  },
});
