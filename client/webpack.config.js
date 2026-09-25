const path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const fs = require("fs");

const DEV_SERVER = "https://localhost:8765/";

// The ASP.NET development certificate, exported by `npm start`. One certificate for the server and
// the watcher means one thing to trust and no warning on either.
const certificate = {
    key: path.resolve(__dirname, ".certs/localhost.key"),
    cert: path.resolve(__dirname, ".certs/localhost.pem"),
};

// Production output stays in the client and is copied into the server's wwwroot by the image.
const dist = path.resolve(__dirname, "dist");

// In development only the shell is written to disk, into the server's wwwroot, so the application is
// served from the server's own origin and the bundles it asks for come from the dev server. The
// session cookie then belongs to the same origin it will in production.
const wwwroot = path.resolve(__dirname, "../server/Codaxy.Inventory/wwwroot");

module.exports = (_env, argv) => {
    const production = argv.mode === "production";

    return {
        entry: "./src/index.tsx",
        output: {
            path: production ? dist : wwwroot,
            filename: production ? "[name].[contenthash:6].js" : "[name].js",
            publicPath: production ? "/" : DEV_SERVER,
            clean: production,
        },
        resolve: {
            extensions: [".ts", ".tsx", ".js"],
        },
        module: {
            rules: [
                {
                    test: /\.[jt]sx?$/,
                    exclude: /node_modules/,
                    use: "babel-loader",
                },
                {
                    test: /\.scss$/,
                    use: [
                        production ? MiniCssExtractPlugin.loader : "style-loader",
                        "css-loader",
                        "sass-loader",
                    ],
                },
                {
                    // The theme ships plain CSS.
                    test: /\.css$/,
                    use: [production ? MiniCssExtractPlugin.loader : "style-loader", "css-loader"],
                },
            ],
        },
        plugins: [
            new HtmlWebpackPlugin({ template: "./src/index.html" }),
            ...(production ? [new MiniCssExtractPlugin({ filename: "[name].[contenthash:6].css" })] : []),
        ],
        devServer: {
            // CxJS replaces the running app through `startHotAppLoop`, so an edit keeps the store and
            // the page you were on instead of reloading it away.
            hot: true,
            port: 8765,
            server: {
                type: "https",
                options: {
                    key: fs.readFileSync(certificate.key),
                    cert: fs.readFileSync(certificate.cert),
                },
            },
            // Only the shell reaches disk; the bundles are served from memory by the dev server, so
            // wwwroot never accumulates a build that would be served instead of them.
            devMiddleware: {
                writeToDisk: (filePath) => filePath.endsWith("index.html"),
            },
            // The page is served by the server on another port, so the dev server has to allow it to
            // fetch bundles and open the hot-reload socket.
            headers: { "Access-Control-Allow-Origin": "*" },
            allowedHosts: "all",
        },
        devtool: production ? "source-map" : "eval-source-map",
    };
};
