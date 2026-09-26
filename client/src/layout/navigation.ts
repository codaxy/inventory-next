import type { NavIconName } from "./icons";

export interface NavItem {
    label: string;
    /** The page heading, where the label alone only makes sense under its section. */
    title: string;
    href: string;
    icon: NavIconName;
}

export interface NavSection {
    title?: string;
    /** The step of `docs/plans/global/0001-the-new-application.md` that builds these screens. */
    step: number;
    items: NavItem[];
}

/** The menu, and the routing table of every screen in it. */
export const navigation: NavSection[] = [
    {
        title: "Electronic devices",
        step: 2,
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
        items: [
            { label: "Furniture", title: "Furniture", href: "~/furniture", icon: "furniture" },
            { label: "Types", title: "Furniture types", href: "~/furniture/types", icon: "furnitureTypes" },
        ],
    },
    {
        title: "Information",
        step: 5,
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
        items: [
            {
                label: "Virtual machines",
                title: "Virtual machines",
                href: "~/infrastructure/virtual-machines",
                icon: "virtualMachines",
            },
            { label: "Clouds", title: "Clouds", href: "~/infrastructure/clouds", icon: "clouds" },
            { label: "Software", title: "Software", href: "~/infrastructure/software", icon: "software" },
        ],
    },
    {
        title: "Directory",
        step: 6,
        items: [
            { label: "People", title: "People", href: "~/directory/people", icon: "people" },
            { label: "Clients", title: "Clients", href: "~/directory/clients", icon: "clients" },
            { label: "Projects", title: "Projects", href: "~/directory/projects", icon: "projects" },
            { label: "Vendors", title: "Vendors", href: "~/directory/vendors", icon: "vendors" },
            {
                label: "Manufacturers",
                title: "Manufacturers",
                href: "~/directory/manufacturers",
                icon: "manufacturers",
            },
            { label: "Locations", title: "Locations", href: "~/directory/locations", icon: "locations" },
        ],
    },
    {
        title: "Administration",
        step: 7,
        items: [
            { label: "Audit log", title: "Audit log", href: "~/administration/audit-log", icon: "auditLog" },
            {
                label: "Server log",
                title: "Server log",
                href: "~/administration/server-log",
                icon: "serverLog",
            },
        ],
    },
];

/** Where `~/` lands: there is no home screen, as in the original. */
export const landing = navigation[0].items[0].href;

const hrefs = navigation.flatMap((section) => section.items.map((item) => item.href));

/**
 * Whether the address is the item's — its own, or a record under it — rather than another item's
 * nested under it: `~/licenses/:id` is Licences', `~/licenses/activations` is Activations'.
 */
export function isCurrent(href: string, url: string | null | undefined): boolean {
    const path = (url ?? "").split("?")[0];
    const under = (base: string) => path === base || path.startsWith(`${base}/`);
    return (
        under(href) && !hrefs.some((other) => other !== href && other.startsWith(`${href}/`) && under(other))
    );
}
