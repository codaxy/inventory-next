import { Controller } from "cx/ui";
import { Button, createHotPromiseWindowFactoryWithProps, Window } from "cx/widgets";

import { historyEntry } from "../historyEntry";

interface Props {
    title: string;
    message: string;
    /** The action, named: "Delete tag", "Discard changes" — never "Yes". */
    confirmText: string;
    /** What declining keeps: "Keep", "Keep editing". */
    cancelText: string;
    /** Red: the action cannot be undone. */
    danger?: boolean;
}

/**
 * A question with two answers that say what they do. Declining is the default: Escape, the backdrop
 * and Back all resolve `false`; only the confirm button resolves `true`.
 */
export const confirm = createHotPromiseWindowFactoryWithProps<Props, boolean>(
    { hot: import.meta.hot },
    ({ title, message, confirmText, cancelText, danger }: Props) =>
        (resolve) => {
            let result = false;
            const backEntry = historyEntry();

            class ConfirmController extends Controller {
                confirm() {
                    result = true;
                    (this.instance as any).dismiss();
                }
            }

            return Window.create(
                <cx>
                    <Window
                        class="confirm-window"
                        title={title}
                        controller={ConfirmController}
                        modal
                        center
                        closable={false}
                        closeOnEscape
                        dismissOnPopState
                        onDestroy={() => backEntry.release(() => resolve(result))}
                    >
                        <p class="confirm-message" text={message} />
                        <div putInto="footer" class="confirm-footer">
                            {/* Focus on declining: Enter must never be the thing that deletes. */}
                            <Button mod="hollow" dismiss text={cancelText} autoFocus />
                            <Button
                                mod="primary"
                                class={{ "confirm-danger": !!danger }}
                                text={confirmText}
                                onClick="confirm"
                            />
                        </div>
                    </Window>
                </cx>,
            );
        },
);
