import { type Config, createFunctionalComponent, expr, falsy, truthy } from "cx/ui";
import { Button, TextField, ValidationGroup } from "cx/widgets";

import Controller from "./Controller";
import m from "./model";

const noProvider = expr(m.signin.providers, (p) => !p.google && !p.oneTimeCode);
// Once a code is on its way, Google is a different path, not an alternative to typing it.
const showGoogle = expr(m.signin.providers, m.signin.codeSent, (p, sent) => p.google && !sent);
const showSeparator = expr(
    m.signin.providers,
    m.signin.codeSent,
    (p, sent) => p.google && p.oneTimeCode && !sent,
);
// A field turns red and says nothing: its own errors — a malformed address, a non-digit in the
// code — are plain on sight. `Field` skips the tooltip for `false`, although its type admits only a config.
const noErrorText = false as unknown as Config;
// Empty is not an error, only a reason the button cannot be pressed yet.
const cannotRequest = expr(
    m.signin.email,
    m.signin.invalid,
    m.signin.busy,
    (email, invalid, busy) => !email || invalid || busy,
);
const cannotVerify = expr(m.signin.code, m.signin.busy, (code, busy) => !/^\d{6}$/.test(code ?? "") || busy);

export default createFunctionalComponent(() => (
    <cx>
        <div class="page sign-in-page" controller={Controller}>
            <div class="card sign-in">
                <div class="brand-mark" />

                <h1 visible={falsy(m.signin.codeSent)} text="Sign in to Inventory" />

                <h1 visible={m.signin.codeSent} text="Check your email" />

                <p class="sent-to" visible={m.signin.codeSent}>
                    Enter the code we sent to
                    <strong text={expr(m.signin.email, (email) => email ?? "")} />
                </p>

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
                <a href="/auth/google/start" class="cxb-button" visible={showGoogle}>
                    <span class="google-logo" />
                    <span text="Continue with Google" />
                </a>

                <div class="separator" visible={showSeparator} text="or" />

                {/* `ValidationGroup` renders no element, so the layout is this div's. */}
                <div class="sign-in-form" visible={m.signin.providers.oneTimeCode}>
                    <ValidationGroup invalid={m.signin.invalid}>
                        <TextField
                            value={m.signin.email}
                            inputType="email"
                            placeholder="Email address"
                            inputAttrs={{ "aria-label": "Email address" }}
                            visible={falsy(m.signin.codeSent)}
                            style="width: 100%"
                            validationRegExp={/^[^\s@]+@[^\s@]+\.[^\s@]+$/}
                            error={m.signin.emailError}
                            errorTooltip={noErrorText}
                        />

                        <p
                            class="field-message"
                            visible={truthy(m.signin.emailError)}
                            text={m.signin.emailError}
                        />

                        <Button
                            mod="primary"
                            onClick="onRequestCode"
                            visible={falsy(m.signin.codeSent)}
                            disabled={cannotRequest}
                            text="Continue with email"
                            style="width: 100%"
                        />

                        {/*
                            Text, not a NumberField: a code is six characters, not a quantity — no
                            separators, and a leading zero is kept. Red only for a non-digit.
                        */}
                        <TextField
                            class="code-input"
                            value={m.signin.code}
                            placeholder="Six-digit code"
                            inputAttrs={{
                                "aria-label": "Six-digit code",
                                autoComplete: "one-time-code",
                                inputMode: "numeric",
                                maxLength: 6,
                            }}
                            validationRegExp={/^\d*$/}
                            error={m.signin.codeError}
                            errorTooltip={noErrorText}
                            visible={m.signin.codeSent}
                            style="width: 100%"
                        />

                        <p
                            class="field-message"
                            visible={truthy(m.signin.codeError)}
                            text={m.signin.codeError}
                        />

                        <Button
                            mod="primary"
                            onClick="onVerifyCode"
                            visible={m.signin.codeSent}
                            disabled={cannotVerify}
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
