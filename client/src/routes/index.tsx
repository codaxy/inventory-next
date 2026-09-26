import { equal, FirstVisibleChildLayout, falsy } from "cx/ui";
import { PureContainer, RedirectRoute, Route } from "cx/widgets";

import { TodoScreen } from "../components/TodoScreen";
import { AppLayout } from "../layout";
import { landing, navigation } from "../layout/navigation";
import $app from "../model";
import AuditLog from "./administration/audit-log";
import ServerLog from "./administration/server-log";
import TagEditor from "./electronic-devices/tags/editor";
import Tags from "./electronic-devices/tags";
import TypeEditor from "./electronic-devices/types/editor";
import Furniture from "./furniture";
import FurnitureEditor from "./furniture/editor";
import FurnitureTypes from "./furniture/types";
import FurnitureTypeEditor from "./furniture/types/editor";
import Licenses from "./licenses";
import ActivationEditor from "./licenses/activations/editor";
import Activations from "./licenses/activations";
import LicenseEditor from "./licenses/editor";
import SoftwareServiceEditor from "./licenses/software-services/editor";
import SoftwareServices from "./licenses/software-services";
import Types from "./electronic-devices/types";
import Controller from "./Controller";
import NotFound from "./not-found";
import SignIn from "./sign-in";

/** The menu items that have a screen; the rest route to a placeholder naming the step that builds them. */
const screens: Record<string, any> = {
    "~/administration/audit-log": AuditLog,
    "~/administration/server-log": ServerLog,
    "~/electronic-devices/tags": Tags,
    "~/electronic-devices/types": Types,
    "~/furniture": Furniture,
    "~/furniture/types": FurnitureTypes,
    "~/licenses": Licenses,
    "~/licenses/activations": Activations,
    "~/licenses/software-services": SoftwareServices,
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

                    {/* Editors, after the menu's own routes: `~/electronic-devices/tags` is one of them. */}
                    <Route route="~/electronic-devices/tags/:id/edit" url={$app.url}>
                        <TagEditor />
                    </Route>
                    <Route route="~/electronic-devices/tags/:id" url={$app.url}>
                        <TagEditor />
                    </Route>
                    <Route route="~/electronic-devices/types/:id/edit" url={$app.url}>
                        <TypeEditor />
                    </Route>
                    <Route route="~/electronic-devices/types/:id" url={$app.url}>
                        <TypeEditor />
                    </Route>
                    {/* Before `~/furniture/:id`, which would take `types` for an id. */}
                    <Route route="~/furniture/types/:id/edit" url={$app.url}>
                        <FurnitureTypeEditor />
                    </Route>
                    <Route route="~/furniture/types/:id" url={$app.url}>
                        <FurnitureTypeEditor />
                    </Route>
                    <Route route="~/furniture/:id/edit" url={$app.url}>
                        <FurnitureEditor />
                    </Route>
                    <Route route="~/furniture/:id" url={$app.url}>
                        <FurnitureEditor />
                    </Route>
                    {/* Before `~/licenses/:id`, which would take `activations` and `software-services` for an id. */}
                    <Route route="~/licenses/activations/:id" url={$app.url}>
                        <ActivationEditor />
                    </Route>
                    <Route route="~/licenses/software-services/:id/edit" url={$app.url}>
                        <SoftwareServiceEditor />
                    </Route>
                    <Route route="~/licenses/software-services/:id" url={$app.url}>
                        <SoftwareServiceEditor />
                    </Route>
                    <Route route="~/licenses/:id/edit" url={$app.url}>
                        <LicenseEditor />
                    </Route>
                    <Route route="~/licenses/:id" url={$app.url}>
                        <LicenseEditor />
                    </Route>

                    <NotFound />
                </PureContainer>
            </AppLayout>
        </PureContainer>
    </cx>
);
