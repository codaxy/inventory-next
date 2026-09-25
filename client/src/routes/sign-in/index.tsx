import { Button, NumberField, TextField, ValidationGroup } from "cx/widgets";

import Controller from "./Controller";

export default () => (
    <cx>
        <div class="page" controller={Controller}>
            <div class="card sign-in">
                <h1>Inventory</h1>

                <p visible-expr="{providers.google} || {providers.oneTimeCode}">
                    Please sign in to access your account.
                </p>

                <p visible-expr="!{providers.google} && !{providers.oneTimeCode}">
                    No sign-in method is configured on this server.
                </p>

                <p class="error" visible-expr="!!{error}" text-bind="error" />

                {/*
                    A plain anchor, not CxJS's Link: Link routes a local href through the client
                    router, and this one has to leave the application and reach the server.
                */}
                <a href="/auth/google/start" class="cxb-button cxm-soft" visible-expr="{providers.google}">
                    <span class="google-logo" />
                    Sign in with Google
                </a>

                <div class="separator" visible-expr="{providers.google} && {providers.oneTimeCode}">
                    or
                </div>

                <ValidationGroup visible-expr="{providers.oneTimeCode}" invalid-bind="invalid" class="stack">
                    <p visible-expr="!{codeSent}">Sign in using your email.</p>

                    <p visible-expr="{codeSent}" text-tpl="We sent a six-digit code to {email}." />

                    <TextField
                        value-bind="email"
                        inputType="email"
                        placeholder="Enter your email"
                        visible-expr="!{codeSent}"
                        style="width: 100%"
                        required
                    />

                    <Button
                        mod="soft"
                        onClick="onRequestCode"
                        visible-expr="!{codeSent}"
                        disabled-expr="{invalid} || {busy}"
                        text="Continue with email"
                        style="width: 100%"
                    />

                    <NumberField
                        value-bind="code"
                        placeholder="Enter the code"
                        format="n;0"
                        visible-expr="{codeSent}"
                        style="width: 100%"
                    />

                    <Button
                        mod="soft"
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
