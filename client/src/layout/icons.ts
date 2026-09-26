import {
    Undo02Icon,
    Add01Icon,
    AppStoreIcon,
    ArrowLeft01Icon,
    ArrowRight01Icon,
    ArrowUp01Icon,
    Briefcase01Icon,
    Building03Icon,
    Cancel01Icon,
    CancelCircleIcon,
    Chair01Icon,
    CheckmarkBadge01Icon,
    Clock01Icon,
    Copy01Icon,
    CloudIcon,
    CommandLineIcon,
    ComputerIcon,
    ConstructionIcon,
    Delete02Icon,
    Factory01Icon,
    FilterHorizontalIcon,
    HistoryIcon,
    InformationCircleIcon,
    Key01Icon,
    Location01Icon,
    Logout01Icon,
    PencilEdit01Icon,
    RefreshIcon,
    Search01Icon,
    ServerStack01Icon,
    Shapes01Icon,
    SortingDownIcon,
    SortingUpIcon,
    SoftwareIcon,
    Store01Icon,
    Tag01Icon,
    UserIcon,
} from "@hugeicons/core-free-icons";

/**
 * HugeIcons' free set (MIT) ships each icon as `[tag, attrs]` tuples with no type of its own, so the
 * shape is declared here.
 */
export type IconData = readonly (readonly [string, Readonly<Record<string, string | number>>])[];

/**
 * One name per use, not per glyph: two items sharing a shape still get their own key, so changing one
 * never silently changes the other.
 */
const navIcons = {
    electronicDevices: ComputerIcon,
    electronicDeviceTypes: Shapes01Icon,
    electronicDeviceTags: Tag01Icon,
    licenses: Key01Icon,
    activations: CheckmarkBadge01Icon,
    softwareServices: AppStoreIcon,
    furniture: Chair01Icon,
    furnitureTypes: Shapes01Icon,
    information: InformationCircleIcon,
    informationTypes: Shapes01Icon,
    informationTags: Tag01Icon,
    virtualMachines: ServerStack01Icon,
    clouds: CloudIcon,
    software: SoftwareIcon,
    people: UserIcon,
    clients: Building03Icon,
    projects: Briefcase01Icon,
    vendors: Store01Icon,
    manufacturers: Factory01Icon,
    locations: Location01Icon,
    auditLog: HistoryIcon,
    serverLog: CommandLineIcon,
} satisfies Record<string, IconData>;

/** Everything that is not a menu item. */
const uiIcons = {
    todo: ConstructionIcon,
    accountMenu: ArrowUp01Icon,
    signOut: Logout01Icon,
    previous: ArrowLeft01Icon,
    next: ArrowRight01Icon,
    search: Search01Icon,
    filters: FilterHorizontalIcon,
    close: Cancel01Icon,
    newestFirst: SortingDownIcon,
    oldestFirst: SortingUpIcon,
    recordHistory: Clock01Icon,
    created: Add01Icon,
    updated: PencilEdit01Icon,
    deleted: Delete02Icon,
    refresh: RefreshIcon,
    edit: PencilEdit01Icon,
    delete: Delete02Icon,
    duplicate: Copy01Icon,
    deactivate: CancelCircleIcon,
    reactivate: Undo02Icon,
} satisfies Record<string, IconData>;

export const icons = { ...navIcons, ...uiIcons };

export type IconName = keyof typeof icons;
export type NavIconName = keyof typeof navIcons;
