import fs from "node:fs";
import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig, type Plugin } from "vite";

const DEV_SERVER = "https://localhost:8765";

// The ASP.NET development certificate, exported by `npm start`. One certificate for the server and
// the dev server means one thing to trust and no warning on either. Read only when serving: a
// production build runs where the export never happened.
const certificate = {
    key: path.resolve(__dirname, ".certs/localhost.key"),
    cert: path.resolve(__dirname, ".certs/localhost.pem"),
};

const wwwroot = path.resolve(__dirname, "../server/Codaxy.Inventory/wwwroot");

/**
 * In development the page is still served by the server, on its own origin, so the session cookie
 * belongs to the origin it will in production. This writes the server a copy of `index.html` whose
 * scripts come from the dev server; everything else is served from the dev server's memory.
 */
function serverShell(): Plugin {
    return {
        name: "server-shell",
        apply: "serve",
        async configureServer(server) {
            const html = await server.transformIndexHtml(
                "/index.html",
                fs.readFileSync(path.resolve(__dirname, "index.html"), "utf8"),
            );
            fs.writeFileSync(path.join(wwwroot, "index.html"), html.replace(/(src|href)="\//g, `$1="${DEV_SERVER}/`));
        },
    };
}

export default defineConfig(({ command }) => ({
    plugins: [tailwindcss(), serverShell()],
    // CxJS's JSX runtime compiles `<cx>` blocks; TypeScript reads the same setting from tsconfig.
    oxc: {
        jsx: { runtime: "automatic", importSource: "cx" },
    },
    optimizeDeps: {
        include: ["cx/ui", "cx/widgets", "cx/data", "cx/util"],
    },
    server: {
        port: 8765,
        strictPort: true,
        https:
            command === "serve"
                ? { key: fs.readFileSync(certificate.key), cert: fs.readFileSync(certificate.cert) }
                : undefined,
        // Asset URLs are absolute, and module requests cross from the server's origin to this one.
        origin: DEV_SERVER,
        cors: true,
    },
    build: {
        outDir: "dist",
        emptyOutDir: true,
    },
}));
