import { Button, NumberField, TextField, ValidationGroup } from "cx/widgets";

import Controller from "./Controller";

export default () => (
    <cx>
        <div class="page" controller={Controller}>
            <div class="card">
                <h1>Inventory</h1>

                <p visible-expr="!{providers.google} && !{providers.oneTimeCode}">
                    No sign-in method is configured on this server.
                </p>

                <p class="error" visible-expr="!!{error}" text-bind="error" />

                {/*
                    A plain anchor, not CxJS's Link: Link routes a local href through the client
                    router, and this one has to leave the application and reach the server.
                */}
                <a href="/auth/google/start" class="cxb-button cxm-primary" visible-expr="{providers.google}">
                    Continue with Google
                </a>

                <div class="separator" visible-expr="{providers.google} && {providers.oneTimeCode}">
                    or
                </div>

                <ValidationGroup visible-expr="{providers.oneTimeCode}" invalid-bind="invalid" class="stack">
                    <TextField
                        value-bind="email"
                        label="Email"
                        inputType="email"
                        placeholder="you@codaxy.com"
                        enabled-expr="!{codeSent}"
                        style="width: 100%"
                        required
                    />

                    <Button
                        mod="primary"
                        onClick="onRequestCode"
                        visible-expr="!{codeSent}"
                        disabled-expr="{invalid} || {busy}"
                        text="Email me a code"
                        style="width: 100%"
                    />

                    <NumberField
                        value-bind="code"
                        label="Six-digit code"
                        format="n;0"
                        visible-expr="{codeSent}"
                        style="width: 100%"
                    />

                    <Button
                        mod="primary"
                        onClick="onVerifyCode"
                        visible-expr="{codeSent}"
                        disabled-expr="{busy}"
                        text="Sign in"
                        style="width: 100%"
                    />

                    <Button
                        mod="hollow"
                        onClick="onStartOver"
                        visible-expr="{codeSent}"
                        text="Use a different address"
                        style="width: 100%"
                    />
                </ValidationGroup>
            </div>
        </div>
    </cx>
);
