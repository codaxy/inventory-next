import { Controller } from "cx/ui";

import { getDashboard } from "../../api/dashboard";
import { ApiError } from "../../api/http";
import m, { firstTile, type Tile, toDashboard } from "./model";

export default class extends Controller {
    onInit() {
        this.store.set(m.dashboard, {
            loaded: false,
            tiles: [],
            detail: [],
        });
        this.load();
    }

    async load() {
        this.store.delete(m.dashboard.error);
        try {
            const tiles = toDashboard(await getDashboard());
            this.store.set(m.dashboard.tiles, tiles);
            this.select(this.store.get(m.dashboard.selected) ?? firstTile(tiles));
        } catch (error) {
            this.store.set(
                m.dashboard.error,
                error instanceof ApiError ? error.message : "The dashboard could not be loaded.",
            );
        } finally {
            this.store.set(m.dashboard.loaded, true);
        }
    }

    /** Open a tile's rows in the card beneath; a tile with nothing opens nothing. */
    select(key: string | undefined) {
        const tile = this.store.get(m.dashboard.tiles).find((t: Tile) => t.key === key);
        if (!tile || tile.empty) return;
        this.store.set(m.dashboard.selected, key);
        this.store.set(m.dashboard.detail, tile.section ? [tile.section] : []);
    }
}
