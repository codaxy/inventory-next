import { equal, FirstVisibleChildLayout, falsy } from "cx/ui";
import { PureContainer, RedirectRoute, Route } from "cx/widgets";

import { TodoScreen } from "../components/TodoScreen";
import { AppLayout } from "../layout";
import { landing, navigation } from "../layout/navigation";
import $app from "../model";
import AuditLog from "./administration/audit-log";
import Clients from "./company/clients";
import ClientEditor from "./company/clients/editor";
import Locations from "./company/locations";
import LocationEditor from "./company/locations/editor";
import Manufacturers from "./company/manufacturers";
import ManufacturerEditor from "./company/manufacturers/editor";
import People from "./company/people";
import PersonEditor from "./company/people/editor";
import Handover from "./company/people/handover";
import Projects from "./company/projects";
import ProjectEditor from "./company/projects/editor";
import Vendors from "./company/vendors";
import VendorEditor from "./company/vendors/editor";
import ServerLog from "./administration/server-log";
import TagEditor from "./electronic-devices/tags/editor";
import Tags from "./electronic-devices/tags";
import TypeEditor from "./electronic-devices/types/editor";
import Furniture from "./furniture";
import FurnitureEditor from "./furniture/editor";
import FurnitureTypes from "./furniture/types";
import FurnitureTypeEditor from "./furniture/types/editor";
import Informations from "./informations";
import InformationEditor from "./informations/editor";
import InformationTags from "./informations/tags";
import InformationTagEditor from "./informations/tags/editor";
import InformationTypes from "./informations/types";
import InformationTypeEditor from "./informations/types/editor";
import CloudSubscriptions from "./infrastructure/cloud-subscriptions";
import CloudSubscriptionEditor from "./infrastructure/cloud-subscriptions/editor";
import Softwares from "./infrastructure/software";
import SoftwareEditor from "./infrastructure/software/editor";
import VirtualMachines from "./infrastructure/virtual-machines";
import VirtualMachineEditor from "./infrastructure/virtual-machines/editor";
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
    "~/company/clients": Clients,
    "~/company/locations": Locations,
    "~/company/manufacturers": Manufacturers,
    "~/company/people": People,
    "~/company/projects": Projects,
    "~/company/vendors": Vendors,
    "~/electronic-devices/tags": Tags,
    "~/electronic-devices/types": Types,
    "~/furniture": Furniture,
    "~/furniture/types": FurnitureTypes,
    "~/informations": Informations,
    "~/informations/tags": InformationTags,
    "~/informations/types": InformationTypes,
    "~/infrastructure/cloud-subscriptions": CloudSubscriptions,
    "~/infrastructure/software": Softwares,
    "~/infrastructure/virtual-machines": VirtualMachines,
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
                    <Route route="~/company/projects/:id/edit" url={$app.url}>
                        <ProjectEditor />
                    </Route>
                    <Route route="~/company/projects/:id" url={$app.url}>
                        <ProjectEditor />
                    </Route>
                    <Route route="~/company/vendors/:id/edit" url={$app.url}>
                        <VendorEditor />
                    </Route>
                    <Route route="~/company/vendors/:id" url={$app.url}>
                        <VendorEditor />
                    </Route>
                    <Route route="~/company/manufacturers/:id/edit" url={$app.url}>
                        <ManufacturerEditor />
                    </Route>
                    <Route route="~/company/manufacturers/:id" url={$app.url}>
                        <ManufacturerEditor />
                    </Route>
                    <Route route="~/company/locations/:id/edit" url={$app.url}>
                        <LocationEditor />
                    </Route>
                    <Route route="~/company/locations/:id" url={$app.url}>
                        <LocationEditor />
                    </Route>
                    <Route route="~/company/clients/:id/edit" url={$app.url}>
                        <ClientEditor />
                    </Route>
                    <Route route="~/company/clients/:id" url={$app.url}>
                        <ClientEditor />
                    </Route>
                    <Route route="~/company/people/:id/handover" url={$app.url}>
                        <Handover />
                    </Route>
                    <Route route="~/company/people/:id/edit" url={$app.url}>
                        <PersonEditor />
                    </Route>
                    <Route route="~/company/people/:id" url={$app.url}>
                        <PersonEditor />
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
                    <Route route="~/infrastructure/virtual-machines/:id/edit" url={$app.url}>
                        <VirtualMachineEditor />
                    </Route>
                    <Route route="~/infrastructure/virtual-machines/:id" url={$app.url}>
                        <VirtualMachineEditor />
                    </Route>
                    <Route route="~/infrastructure/cloud-subscriptions/:id/edit" url={$app.url}>
                        <CloudSubscriptionEditor />
                    </Route>
                    <Route route="~/infrastructure/cloud-subscriptions/:id" url={$app.url}>
                        <CloudSubscriptionEditor />
                    </Route>
                    <Route route="~/infrastructure/software/:id/edit" url={$app.url}>
                        <SoftwareEditor />
                    </Route>
                    <Route route="~/infrastructure/software/:id" url={$app.url}>
                        <SoftwareEditor />
                    </Route>
                    {/* Before `~/informations/:id`, which would take `types` and `tags` for an id. */}
                    <Route route="~/informations/types/:id/edit" url={$app.url}>
                        <InformationTypeEditor />
                    </Route>
                    <Route route="~/informations/types/:id" url={$app.url}>
                        <InformationTypeEditor />
                    </Route>
                    <Route route="~/informations/tags/:id/edit" url={$app.url}>
                        <InformationTagEditor />
                    </Route>
                    <Route route="~/informations/tags/:id" url={$app.url}>
                        <InformationTagEditor />
                    </Route>
                    <Route route="~/informations/:id/edit" url={$app.url}>
                        <InformationEditor />
                    </Route>
                    <Route route="~/informations/:id" url={$app.url}>
                        <InformationEditor />
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
