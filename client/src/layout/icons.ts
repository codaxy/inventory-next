import {
    AppStoreIcon,
    Briefcase01Icon,
    Building03Icon,
    CheckmarkBadge01Icon,
    Chair01Icon,
    CloudIcon,
    CommandLineIcon,
    ComputerIcon,
    ConstructionIcon,
    Factory01Icon,
    HistoryIcon,
    InformationCircleIcon,
    Key01Icon,
    Location01Icon,
    ServerStack01Icon,
    Shapes01Icon,
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
export const icons = {
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
    todo: ConstructionIcon,
} satisfies Record<string, IconData>;

export type IconName = keyof typeof icons;
export type NavIconName = Exclude<IconName, "todo">;
