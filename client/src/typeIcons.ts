import type { IconName } from "./layout/icons";

/** A kind of type that carries an icon. */
export type IconKind = "device" | "furniture";

/** An icon a type may carry: the key the server keeps, what the picker calls it, its glyph. */
export interface TypeIcon {
    key: string;
    label: string;
    glyph: IconName;
}

/**
 * The icons a device type may carry, the keys `TypeIcons.Device` on the server allows, in the
 * picker's order: computers and screens, what plugs into them, then the rest.
 */
export const deviceIcons: TypeIcon[] = [
    { key: "laptop", label: "Laptop", glyph: "thingLaptop" },
    { key: "desktop", label: "Desktop", glyph: "thingDesktop" },
    { key: "parts", label: "Parts", glyph: "thingParts" },
    { key: "monitor", label: "Monitor", glyph: "thingMonitor" },
    { key: "tablet", label: "Tablet", glyph: "thingTablet" },
    { key: "phone", label: "Phone", glyph: "thingPhone" },
    { key: "watch", label: "Watch", glyph: "thingWatch" },
    { key: "dock", label: "Docking station", glyph: "thingDock" },
    { key: "hub", label: "Hub", glyph: "thingHub" },
    { key: "keyboard", label: "Keyboard", glyph: "thingKeyboard" },
    { key: "mouse", label: "Mouse", glyph: "thingMouse" },
    { key: "pen", label: "Pen", glyph: "thingPen" },
    { key: "headset", label: "Headset", glyph: "thingHeadset" },
    { key: "earbuds", label: "Earbuds", glyph: "thingEarbuds" },
    { key: "microphone", label: "Microphone", glyph: "thingMicrophone" },
    { key: "speaker", label: "Speaker", glyph: "thingSpeaker" },
    { key: "webcam", label: "Webcam", glyph: "thingWebcam" },
    { key: "camera", label: "Camera", glyph: "thingCamera" },
    { key: "cctv", label: "CCTV", glyph: "thingCctv" },
    { key: "printer", label: "Printer", glyph: "thingPrinter" },
    { key: "projector", label: "Projector", glyph: "thingProjector" },
    { key: "presenter", label: "Presenter", glyph: "thingPresenter" },
    { key: "router", label: "Router or switch", glyph: "thingRouter" },
    { key: "wifi", label: "Access point", glyph: "thingWifi" },
    { key: "firewall", label: "Firewall", glyph: "thingFirewall" },
    { key: "server", label: "Server", glyph: "thingServer" },
    { key: "storage", label: "Storage", glyph: "thingStorage" },
    { key: "power", label: "Power", glyph: "thingPower" },
    { key: "console", label: "Console", glyph: "thingConsole" },
    { key: "vr", label: "VR", glyph: "thingVr" },
    { key: "backpack", label: "Backpack", glyph: "thingBackpack" },
    { key: "fridge", label: "Fridge", glyph: "thingFridge" },
    { key: "air", label: "Air", glyph: "thingAir" },
    { key: "coffee", label: "Coffee", glyph: "thingCoffee" },
];

/** The icons a furniture type may carry, as `TypeIcons.Furniture` allows. */
export const furnitureIcons: TypeIcon[] = [
    { key: "armchair", label: "Armchair", glyph: "thingArmchair" },
    { key: "sofa", label: "Sofa", glyph: "thingSofa" },
    { key: "desk", label: "Desk", glyph: "thingDesk" },
    { key: "table", label: "Table", glyph: "thingTable" },
    { key: "dresser", label: "Dresser", glyph: "thingDresser" },
    { key: "drawers", label: "Drawers", glyph: "thingDrawers" },
    { key: "wardrobe", label: "Wardrobe", glyph: "thingWardrobe" },
    { key: "shelf", label: "Shelf", glyph: "thingShelf" },
    { key: "lamp", label: "Lamp", glyph: "thingLamp" },
    { key: "whiteboard", label: "Whiteboard", glyph: "thingWhiteboard" },
    { key: "safe", label: "Safe", glyph: "thingSafe" },
    { key: "hanger", label: "Coat stand", glyph: "thingHanger" },
];

/** No icon chosen: the menu's for the kind. */
export const genericIcon: Record<IconKind, TypeIcon> = {
    device: { key: "", label: "Generic", glyph: "electronicDevices" },
    furniture: { key: "", label: "Generic", glyph: "furniture" },
};

export const iconsOf = (kind: IconKind) => (kind === "device" ? deviceIcons : furnitureIcons);

/** The glyph for a type's icon key; none, or one no longer known, is the generic one. */
export const glyphOf = (kind: IconKind, key: string | null | undefined): IconName =>
    iconsOf(kind).find((i) => i.key === key)?.glyph ?? genericIcon[kind].glyph;

/**
 * A new type's icon from its name, in English or the office's own language: the first match wins, so
 * "microphone" is not a phone and "KVM console" is a switch. The only guesser: here, so leaving the
 * name costs no request. The seeded types carry their icons as data; the migration that filled the
 * first ones guessed the same way once, in SQL, and stays as it was.
 */
const devicePatterns: [RegExp, string][] = [
    [/microphone|mikrofon/, "microphone"],
    [/laptop|notebook|prijenosn/, "laptop"],
    [/server|\brack/, "server"],
    [/drive|\bssd|\bhdd|storage|\bnas\b/, "storage"],
    [/component|komponent|\bparts?\b|dijelov/, "parts"],
    [/desktop|computer|\bpc\b|ra[cč]unar|workstation/, "desktop"],
    [/monitor|display|screen|\btv\b|televizor|ekran/, "monitor"],
    [/dock/, "dock"],
    [/\bhub\b|\busb/, "hub"],
    [/headset|slu[sš]alic/, "headset"],
    [/earbud|airpod|headphone/, "earbuds"],
    [/keyboard|tastatur/, "keyboard"],
    // Not "\b" around "miš": JavaScript's word boundary knows no "š".
    [/mouse|(^|\s)mi[sš]($|\s)/, "mouse"],
    [/webcam|web cam/, "webcam"],
    [/cctv|surveillance|nadzor/, "cctv"],
    [/camera|kamera|fotoaparat/, "camera"],
    [/smart ?watch|\bwatch/, "watch"],
    [/phone|mobitel|telefon/, "phone"],
    [/tablet|\bipad/, "tablet"],
    [/pencil|stylus|olovk/, "pen"],
    [/print|mfp|copier|kopir|scanner|skener|\bfax|[sš]tampa[cč]/, "printer"],
    [/access point|wi-?fi|\bap\b/, "wifi"],
    [/firewall/, "firewall"],
    [/router|ruter|switch|gateway|modem|bridge|\bkvm/, "router"],
    [/speaker|zvu[cč]nik/, "speaker"],
    [/\bups\b|power|napaja/, "power"],
    [/console|playstation|xbox|gaming/, "console"],
    [/\bvr\b|virtual reality|oculus/, "vr"],
    [/projector|projektor|beamer/, "projector"],
    [/prezenter|presenter|clicker/, "presenter"],
    [/backpack|ranac|\bbag\b/, "backpack"],
    [/fridge|fri[zž]ider|hladnjak|vitrina|refrigerat/, "fridge"],
    [/air purifier|pre[cč]i[sš][cć]iva[cč]|\bfan\b|ventilator|air condition|klima/, "air"],
    [/coffee|kafe|espresso/, "coffee"],
];

const furniturePatterns: [RegExp, string][] = [
    // A chair is the generic icon; matched still, so that "stolica" is not taken for "stolić".
    [/stolica|chair/, ""],
    [/fotelja|armchair/, "armchair"],
    [/trosjed|dvosjed|garnitura|sofa|couch|kau[cč]/, "sofa"],
    [/radni sto|desk|pisa[cć]i/, "desk"],
    [/komoda|dresser/, "dresser"],
    [/ladi[cč]ar|drawer|kontejner/, "drawers"],
    [/ormar|wardrobe|cabinet|plakar/, "wardrobe"],
    [/poli[cč]ar|polica|shel|regal|bookcase/, "shelf"],
    [/stoli[cć]|(^|\s)sto($|\s)|(^|\s)stol($|\s)|table/, "table"],
    [/lamp|svjetiljka/, "lamp"],
    [/whiteboard|tabla|plo[cč]a/, "whiteboard"],
    [/(^|\s)sef($|\s)|\bsafe\b/, "safe"],
    [/[cč]iviluk|coat|hanger|vje[sš]alic/, "hanger"],
];

/**
 * The icon a type's name suggests: a key, `null` for the generic one, or `undefined` where the name
 * suggests nothing — the picker keeps whatever it has then.
 */
export function guessIcon(kind: IconKind, name: string | null | undefined): string | null | undefined {
    const text = (name ?? "").trim().toLowerCase();
    if (!text) return undefined;
    const patterns = kind === "device" ? devicePatterns : furniturePatterns;
    const found = patterns.find(([pattern]) => pattern.test(text));
    return found ? found[1] || null : undefined;
}
