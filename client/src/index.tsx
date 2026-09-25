import { Store } from "cx/data";
import { History, startHotAppLoop } from "cx/ui";

import "cx-theme-aquamarine/dist/reset.css";
import "cx-theme-aquamarine/dist/widgets.css";
import "./index.scss";

import $app from "./model";
import Routes from "./routes";

const store = new Store();

// `connect` takes a path, not an accessor.
History.connect(store, $app.url.toString());

startHotAppLoop(module, document.getElementById("app")!, store, Routes);
