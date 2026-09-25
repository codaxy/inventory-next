import { createFunctionalComponent } from "cx/ui";
import { Button } from "cx/widgets";

import $app from "../../model";
import Controller from "./Controller";

export default createFunctionalComponent(() => (
    <cx>
        <div class="page" controller={Controller}>
            <div class="card">
                <h1 text="Signed in" />
                <p>
                    <span text={$app.session.user.name} /> — <span text={$app.session.user.email} />
                </p>
                <p text="There is nothing here yet. That is the next step." />
                <Button mod="primary" onClick="onSignOut" text="Sign out" style="width: 100%" />
            </div>
        </div>
    </cx>
));
