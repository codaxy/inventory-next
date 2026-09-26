import { equal, FirstVisibleChildLayout, falsy } from "cx/ui";
import { PureContainer, RedirectRoute, Route } from "cx/widgets";

import { TodoScreen } from "../components/TodoScreen";
import { AppLayout } from "../layout";
import { landing, navigation } from "../layout/navigation";
import $app from "../model";
import AuditLog from "./administration/audit-log";
import ServerLog from "./administration/server-log";
import Controller from "./Controller";
import NotFound from "./not-found";
import SignIn from "./sign-in";

/** The menu items that have a screen; the rest route to a placeholder naming the step that builds them. */
const screens: Record<string, any> = {
    "~/administration/audit-log": AuditLog,
    "~/administration/server-log": ServerLog,
};

// The first matching route wins, so order is the routing table: signed in or not is the outermost
// split, and everything below it can assume the answer.
export default (
    <cx>
        <PureContainer layout={FirstVisibleChildLayout} controller={Controller}>
            <PureContainer visible={equal($app.session.status, "loading")}>
                <div class="page">
                    <p text="Loading…" />
                </div>
            </PureContainer>

            <PureContainer layout={FirstVisibleChildLayout} if={falsy($app.session.user)}>
                <Route route="~/sign-in" url={$app.url}>
                    <SignIn />
                </Route>
                <RedirectRoute route="*any" url={$app.url} redirect="~/sign-in" />
            </PureContainer>

            <AppLayout>
                <PureContainer layout={FirstVisibleChildLayout}>
                    <RedirectRoute route="~/" url={$app.url} redirect={landing} />
                    <RedirectRoute route="~/sign-in" url={$app.url} redirect={landing} />

                    {navigation.flatMap((section) =>
                        section.items.map((item) => {
                            const Screen = screens[item.href];

                            return (
                                <cx>
                                    <Route route={item.href} url={$app.url}>
                                        {Screen ? (
                                            <Screen />
                                        ) : (
                                            <TodoScreen title={item.title} step={section.step} />
                                        )}
                                    </Route>
                                </cx>
                            );
                        }),
                    )}

                    <NotFound />
                </PureContainer>
            </AppLayout>
        </PureContainer>
    </cx>
);
