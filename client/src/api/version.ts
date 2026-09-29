/** The build the server runs: `26.9.29+1222.eaea98a`, or `dev…` where no release stamped it. */
export async function getVersion(): Promise<string> {
    const response = await fetch("/api/version", { credentials: "same-origin" });
    if (!response.ok) throw new Error(`The version could not be read (${response.status}).`);
    return (await response.text()).trim();
}
