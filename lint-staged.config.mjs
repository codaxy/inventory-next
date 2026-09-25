// Formats what is staged, with the same tools and scope CI checks — `dotnet csharpier check server`
// and the client's `format:check` — so a commit cannot fail CI on formatting. lint-staged re-stages
// what it rewrites.
//
// Prettier runs from the client's own install, so there is one version to keep; through `node`
// rather than a `.bin` shim, which differs between WSL and Git for Windows.
const prettier = "node client/node_modules/prettier/bin/prettier.cjs --write";

export default {
    "client/src/**/*.{ts,tsx,scss,html}": prettier,
    "client/*.{json,js}": prettier,
    "server/**/*.{cs,csproj,props,targets}": "dotnet csharpier format",
};
