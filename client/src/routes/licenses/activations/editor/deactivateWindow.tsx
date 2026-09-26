import { Controller, createModel } from "cx/ui";
import {
    Button,
    createHotPromiseWindowFactoryWithProps,
    DateField,
    ValidationGroup,
    Window,
} from "cx/widgets";

import { dateValue } from "../../../../bindings";
import { encodeDate } from "../../../../dates";
import { historyEntry } from "../../../../historyEntry";

interface Props {
    /** The day the activation began: the earliest it can end. */
    activated: string;
}

interface WindowModel {
    w: { date?: string | null; valid: boolean; visited: boolean };
}

const wm = createModel<WindowModel>();

/**
 * The day the seats come back, today unless changed. Resolves the date, or `false` when dismissed —
 * Cancel, Escape, the backdrop and Back alike; the caller's controller does the saving.
 */
export const askDeactivationDate = createHotPromiseWindowFactoryWithProps<Props, string | false>(
    { hot: import.meta.hot },
    ({ activated }: Props) =>
        (resolve) => {
            let result: string | false = false;
            const backEntry = historyEntry();
            const today = encodeDate(new Date());

            class WindowController extends Controller {
                onInit() {
                    this.store.init(wm.w, {
                        date: today < activated ? activated : today,
                        valid: true,
                        visited: false,
                    });
                }

                submit() {
                    if (!this.store.get(wm.w.valid)) return this.store.set(wm.w.visited, true);
                    result = this.store.get(wm.w.date) ?? false;
                    (this.instance as any).dismiss();
                }
            }

            return Window.create(
                <cx>
                    <Window
                        class="confirm-window"
                        title="Deactivate this activation?"
                        controller={WindowController}
                        modal
                        center
                        closable={false}
                        closeOnEscape
                        dismissOnPopState
                        onDestroy={() => backEntry.release(() => resolve(result))}
                    >
                        <p
                            class="confirm-message"
                            text="Its seats go back to the volume from the day you choose."
                        />
                        <ValidationGroup valid={wm.w.valid} visited={wm.w.visited}>
                            <div class="editor-label" text="Deactivated on" />
                            <DateField
                                value={dateValue(wm.w.date)}
                                required
                                minValue={activated}
                                style="width: 100%"
                                inputAttrs={{ "aria-label": "Deactivated on" }}
                            />
                        </ValidationGroup>
                        <div putInto="footer" class="confirm-footer">
                            <Button mod="hollow" dismiss text="Keep active" autoFocus />
                            <Button mod="primary" text="Deactivate" onClick="submit" />
                        </div>
                    </Window>
                </cx>,
            );
        },
);
