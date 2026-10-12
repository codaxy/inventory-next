import type { Permission } from "../api/auth";
import type { NavIconName } from "./icons";

export interface NavItem {
    label: string;
    /** The page heading, where the label alone only makes sense under its section. */
    title: string;
    href: string;
    icon: NavIconName;
    /** The section's, unless the item has its own: Administration's items each do. */
    view?: Permission;
    edit?: Permission;
}

export interface NavSection {
    title?: string;
    /** The step of `docs/plans/global/0001-the-new-application.md` that builds these screens. */
    step: number;
    /** What showing its items, and every page under them, takes. */
    view?: Permission;
    /** What adding, changing and deleting there takes; none where nothing is. */
    edit?: Permission;
    items: NavItem[];
}

/** The menu, and the routing table of every screen in it. */
export const navigation: NavSection[] = [
    {
        // Above every section and in none: where the day starts.
        step: 1,
        view: "dashboard.view",
        items: [{ label: "Dashboard", title: "Dashboard", href: "~/dashboard", icon: "dashboard" }],
    },
    {
        title: "Company",
        step: 6,
        view: "company.view",
        edit: "company.edit",
        items: [
            { label: "People", title: "People", href: "~/company/people", icon: "people" },
            { label: "Clients", title: "Clients", href: "~/company/clients", icon: "clients" },
            { label: "Projects", title: "Projects", href: "~/company/projects", icon: "projects" },
            { label: "Vendors", title: "Vendors", href: "~/company/vendors", icon: "vendors" },
            {
                label: "Manufacturers",
                title: "Manufacturers",
                href: "~/company/manufacturers",
                icon: "manufacturers",
            },
            { label: "Locations", title: "Locations", href: "~/company/locations", icon: "locations" },
        ],
    },
    {
        title: "Electronic devices",
        step: 2,
        view: "electronic_devices.view",
        edit: "electronic_devices.edit",
        items: [
            {
                label: "Electronic devices",
                title: "Electronic devices",
                href: "~/electronic-devices",
                icon: "electronicDevices",
            },
            {
                label: "Types",
                title: "Electronic device types",
                href: "~/electronic-devices/types",
                icon: "electronicDeviceTypes",
            },
            {
                label: "Tags",
                title: "Electronic device tags",
                href: "~/electronic-devices/tags",
                icon: "electronicDeviceTags",
            },
        ],
    },
    {
        title: "Licenses",
        step: 3,
        view: "licenses.view",
        edit: "licenses.edit",
        items: [
            { label: "Licenses", title: "Licenses", href: "~/licenses", icon: "licenses" },
            {
                label: "Activations",
                title: "Activations",
                href: "~/licenses/activations",
                icon: "activations",
            },
            {
                label: "Software & services",
                title: "Software and services",
                href: "~/licenses/software-services",
                icon: "softwareServices",
            },
        ],
    },
    {
        title: "Furniture",
        step: 4,
        view: "furniture.view",
        edit: "furniture.edit",
        items: [
            { label: "Furniture", title: "Furniture", href: "~/furniture", icon: "furniture" },
            { label: "Types", title: "Furniture types", href: "~/furniture/types", icon: "furnitureTypes" },
        ],
    },
    {
        title: "Information",
        step: 5,
        view: "information.view",
        edit: "information.edit",
        items: [
            {
                label: "Information",
                title: "Information",
                href: "~/informations",
                icon: "information",
            },
            {
                label: "Types",
                title: "Information types",
                href: "~/informations/types",
                icon: "informationTypes",
            },
            {
                label: "Tags",
                title: "Information tags",
                href: "~/informations/tags",
                icon: "informationTags",
            },
        ],
    },
    {
        title: "Infrastructure",
        step: 4,
        view: "infrastructure.view",
        edit: "infrastructure.edit",
        items: [
            {
                label: "Virtual machines",
                title: "Virtual machines",
                href: "~/infrastructure/virtual-machines",
                icon: "virtualMachines",
            },
            {
                label: "Cloud subscriptions",
                title: "Cloud subscriptions",
                href: "~/infrastructure/cloud-subscriptions",
                icon: "cloudSubscriptions",
            },
            { label: "Software", title: "Software", href: "~/infrastructure/software", icon: "software" },
        ],
    },
    {
        title: "Administration",
        step: 7,
        items: [
            {
                label: "Users",
                title: "Users",
                href: "~/administration/users",
                icon: "users",
                view: "users.manage",
                edit: "users.manage",
            },
            {
                label: "Audit log",
                title: "Audit log",
                href: "~/administration/audit-log",
                icon: "auditLog",
                view: "audit_log.view",
            },
            {
                label: "Server log",
                title: "Server log",
                href: "~/administration/server-log",
                icon: "serverLog",
                view: "server_log.view",
            },
        ],
    },
];

/** An item with the permissions it takes, its own or its section's. */
export interface Destination extends NavItem {
    view: Permission;
    edit?: Permission;
}

export const destinations: Destination[] = navigation.flatMap((section) =>
    section.items.map((item) => ({
        ...item,
        view: item.view ?? section.view!,
        edit: item.view ? item.edit : section.edit,
    })),
);

/** Where `~/` lands: the first item the reader can see. There is no home screen, as in the original. */
export const landingFor = (permissions: readonly Permission[]) =>
    destinations.find((d) => permissions.includes(d.view))?.href ?? "~/not-found";

const hrefs = navigation.flatMap((section) => section.items.map((item) => item.href));

/** The item an address belongs to — its own page, or a record under it — or none. */
export const destinationOf = (url: string | null | undefined) =>
    destinations.find((d) => isCurrent(d.href, url));

/**
 * Whether the address is the item's — its own, or a record under it — rather than another item's
 * nested under it: `~/licenses/:id` is Licenses', `~/licenses/activations` is Activations'.
 */
export function isCurrent(href: string, url: string | null | undefined): boolean {
    const path = (url ?? "").split("?")[0];
    const under = (base: string) => path === base || path.startsWith(`${base}/`);
    return (
        under(href) && !hrefs.some((other) => other !== href && other.startsWith(`${href}/`) && under(other))
    );
}
