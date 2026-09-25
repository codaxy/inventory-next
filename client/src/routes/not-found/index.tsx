import { createFunctionalComponent } from "cx/ui";
import { Link } from "cx/widgets";

import { landing } from "../../layout/navigation";

export default createFunctionalComponent(() => (
    <cx>
        <div class="page-body">
            <h1 class="page-header page-title" text="Page not found" />
            <p class="page-lede">
                Nothing lives at this address. Check the URL, or go to{" "}
                <Link href={landing} text="the start" />.
            </p>
        </div>
    </cx>
));
