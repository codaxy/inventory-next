import { createFunctionalComponent, expr, falsy, tpl, truthy } from "cx/ui";
import { Button, NumberField, TextField, ValidationGroup } from "cx/widgets";

import { numberValue } from "../../bindings";
import Controller from "./Controller";
import m from "./model";

const noProvider = expr(m.signin.providers, (p) => !p.google && !p.oneTimeCode);
const bothProviders = expr(m.signin.providers, (p) => p.google && p.oneTimeCode);
const cannotRequest = expr(m.signin.invalid, m.signin.busy, (invalid, busy) => invalid || busy);

export default createFunctionalComponent(() => (
    <cx>
        <div class="page" controller={Controller}>
            <div class="card sign-in">
                <div class="brand-mark" />

                <h1 visible={falsy(m.signin.codeSent)} text="Sign in to Inventory" />

                <h1 visible={m.signin.codeSent} text="Check your email" />

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
                <a href="/auth/google/start" class="cxb-button" visible={m.signin.providers.google}>
                    <span class="google-logo" />
                    <span text="Continue with Google" />
                </a>

                <div class="separator" visible={bothProviders} text="or" />

                {/* `ValidationGroup` renders no element, so the layout is this div's. */}
                <div class="sign-in-form" visible={m.signin.providers.oneTimeCode}>
                    <ValidationGroup invalid={m.signin.invalid}>
                        <p
                            visible={m.signin.codeSent}
                            text={tpl(m.signin.email, "We sent a six-digit code to {0}.")}
                        />

                        <TextField
                            value={m.signin.email}
                            inputType="email"
                            placeholder="Email address"
                            inputAttrs={{ "aria-label": "Email address" }}
                            visible={falsy(m.signin.codeSent)}
                            style="width: 100%"
                            required
                        />

                        <Button
                            mod="primary"
                            onClick="onRequestCode"
                            visible={falsy(m.signin.codeSent)}
                            disabled={cannotRequest}
                            text="Continue with email"
                            style="width: 100%"
                        />

                        <NumberField
                            value={numberValue(m.signin.code)}
                            placeholder="Six-digit code"
                            inputAttrs={{ "aria-label": "Six-digit code" }}
                            format="n;0"
                            visible={m.signin.codeSent}
                            style="width: 100%"
                        />

                        <Button
                            mod="primary"
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

            <p class="sign-in-footer">
                <span text="Built by " />
                <a href="https://www.codaxy.com" target="_blank" rel="noopener" text="Codaxy" />
            </p>
        </div>
    </cx>
));
