import { createFunctionalComponent, expr, falsy, tpl, truthy } from "cx/ui";
import { Button, NumberField, TextField, ValidationGroup } from "cx/widgets";

import { numberValue } from "../../bindings";
import Controller from "./Controller";
import m from "./model";

const anyProvider = expr(m.signin.providers, (p) => p.google || p.oneTimeCode);
const noProvider = expr(m.signin.providers, (p) => !p.google && !p.oneTimeCode);
const bothProviders = expr(m.signin.providers, (p) => p.google && p.oneTimeCode);
const cannotRequest = expr(m.signin.invalid, m.signin.busy, (invalid, busy) => invalid || busy);

export default createFunctionalComponent(() => (
    <cx>
        <div class="page" controller={Controller}>
            <div class="card sign-in">
                <h1 text="Inventory" />

                <p visible={anyProvider} text="Please sign in to access your account." />

                <p visible={noProvider} text="No sign-in method is configured on this server." />

                <p
                    class="error"
                    visible={truthy(m.signin.error)}
                    text={expr(m.signin.error, (error) => error ?? "")}
                />

                {/*
                    A plain anchor, not CxJS's Link: Link routes a local href through the client
                    router, and this one has to leave the application and reach the server.
                */}
                <a href="/auth/google/start" class="cxb-button cxm-soft" visible={m.signin.providers.google}>
                    <span class="google-logo" />
                    <span text="Sign in with Google" />
                </a>

                <div class="separator" visible={bothProviders} text="or" />

                <ValidationGroup visible={m.signin.providers.oneTimeCode} invalid={m.signin.invalid}>
                    <p visible={falsy(m.signin.codeSent)} text="Sign in using your email." />

                    <p
                        visible={m.signin.codeSent}
                        text={tpl(m.signin.email, "We sent a six-digit code to {0}.")}
                    />

                    <TextField
                        value={m.signin.email}
                        inputType="email"
                        placeholder="Enter your email"
                        visible={falsy(m.signin.codeSent)}
                        style="width: 100%"
                        required
                    />

                    <Button
                        mod="soft"
                        onClick="onRequestCode"
                        visible={falsy(m.signin.codeSent)}
                        disabled={cannotRequest}
                        text="Continue with email"
                        style="width: 100%"
                    />

                    <NumberField
                        value={numberValue(m.signin.code)}
                        placeholder="Enter the code"
                        format="n;0"
                        visible={m.signin.codeSent}
                        style="width: 100%"
                    />

                    <Button
                        mod="soft"
                        onClick="onVerifyCode"
                        visible={m.signin.codeSent}
                        disabled={m.signin.busy}
                        text="Sign in"
                        style="width: 100%"
                    />

                    <Button
                        mod="hollow"
                        onClick="onStartOver"
                        visible={m.signin.codeSent}
                        text="Use a different address"
                        style="width: 100%"
                    />
                </ValidationGroup>
            </div>
        </div>
    </cx>
));
