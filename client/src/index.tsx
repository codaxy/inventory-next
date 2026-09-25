import "@fontsource/montserrat/400.css";
import "@fontsource/montserrat/500.css";
import "@fontsource/montserrat/600.css";
import "@fontsource/montserrat/700.css";

import { Store } from "cx/data";
import { History, startHotAppLoop } from "cx/ui";
import { renderThemeVariables } from "cx-theme-variables";

import "./tailwind.css";
import "./index.scss";

import $app from "./model";
import Routes from "./routes";
import { theme } from "./theme";

renderThemeVariables(theme);

const store = new Store();

// `connect` takes a path, not an accessor.
History.connect(store, $app.url.toString());

startHotAppLoop(module, document.getElementById("app")!, store, Routes);
