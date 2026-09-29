import { ApiError } from "./api/http";

/**
 * Fetches a file the API serves and saves it under the name the server gives, so the page knows when
 * it arrived and what went wrong — a plain `<a download>` hands both to the browser, which saves an
 * error answer as a broken file.
 */
export async function download(url: string): Promise<void> {
    const response = await fetch(url, { credentials: "same-origin" });
    if (!response.ok) {
        const problem = (await response.json().catch(() => null)) as { title?: string } | null;
        throw new ApiError(problem?.title ?? "Something went wrong.", response.status);
    }

    const blob = await response.blob();
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName(response.headers.get("content-disposition")) ?? "download";
    link.click();
    // Not revoked at once: Safari reads the URL after `click` returns, and a revoked one saves nothing.
    setTimeout(() => URL.revokeObjectURL(link.href), 10_000);
}

/** `filename*=UTF-8''…` when the server sends it — ASP.NET does for any name — else `filename=`. */
function fileName(disposition: string | null): string | undefined {
    if (!disposition) return undefined;
    const encoded = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
    if (encoded) return decodeURIComponent(encoded[1]);
    return /filename="?([^";]+)"?/i.exec(disposition)?.[1];
}
