import type { AccessorChain } from "cx/data";
import { type Config, expr, falsy, hasValue } from "cx/ui";
import { Checkbox, DateField, LookupField, NumberField, TextArea, TextField } from "cx/widgets";

import { dateValue, numberValue } from "../bindings";
import { externalLink } from "./externalLink";

/** The server's message goes in a line under its field; cx's hover tooltip would hide it. */
const noErrorText = false as unknown as Config;

/** What an editor's fields bind: its draft, the pickers' lists, the server's messages, the mode. */
export interface FormState {
    draft: AccessorChain<any>;
    /** The pickers' lists, where the form has pickers. */
    options?: AccessorChain<any>;
    errors: AccessorChain<any>;
    viewing: AccessorChain<boolean>;
    /** The asset's importance as computed from the weights, for `basicInformation`. */
    importance?: AccessorChain<string>;
}

interface Layout {
    required?: boolean;
    wide?: boolean;
}

/**
 * An editor's fields over one state, as a label above each and the server's message beneath:
 * `prefix` names the pickers' ids, which their labels point at. Every asset editor shows
 * `basicInformation()` first, then its own sections from the same helpers.
 */
export function formFields(state: FormState, prefix: string) {
    const d = state.draft as any;
    const o = (state.options ?? {}) as any;
    const errors = state.errors as any;
    const editing = falsy(state.viewing);

    const label = (text: string, required?: boolean, id?: string) => (
        <cx>
            <div
                id={id}
                class={{ "editor-label": true, "editor-required": required ? editing : false }}
                text={text}
            />
        </cx>
    );

    const message = (field: string) => (
        <cx>
            <p class="field-message" visible={hasValue(errors[field])} text={errors[field]} />
        </cx>
    );

    /** A picker over one of the options' lists, bound as `<key>Id` and `<key>Text`. */
    const pick = (text: string, key: string, list: string, opts: Layout = {}) => (
        <cx>
            <div class={{ "editor-wide": !!opts.wide }}>
                {label(text, opts.required, `${prefix}-${key}-label`)}
                <LookupField
                    id={`${prefix}-${key}`}
                    value={d[`${key}Id`]}
                    text={d[`${key}Text`]}
                    options={o[list]}
                    required={!!opts.required}
                    emptyText="—"
                    placeholder="—"
                    error={errors[`${key}Id`]}
                    errorTooltip={noErrorText}
                    inputAttrs={{ "aria-label": text }}
                />
                {message(`${key}Id`)}
            </div>
        </cx>
    );

    /** A text field; a `url` field has its open-in-a-new-tab button at the end of the line. */
    const text = (text: string, key: string, max: number, opts: Layout & { url?: boolean } = {}) => (
        <cx>
            <div class={{ "editor-wide": !!opts.wide }}>
                {label(text, opts.required)}
                <div class={{ "editor-url": !!opts.url }}>
                    <TextField
                        value={d[key]}
                        required={!!opts.required}
                        maxLength={max}
                        emptyText="—"
                        error={errors[key]}
                        errorTooltip={noErrorText}
                        inputAttrs={{ "aria-label": text }}
                    />
                    {opts.url ? externalLink(d[key]) : null}
                </div>
                {message(key)}
            </div>
        </cx>
    );

    /** Prose, across the grid. */
    const prose = (text: string, key: string, max: number) => (
        <cx>
            <div class="editor-wide">
                {label(text)}
                <TextArea
                    value={d[key]}
                    emptyText="—"
                    maxLength={max}
                    rows={3}
                    error={errors[key]}
                    errorTooltip={noErrorText}
                    inputAttrs={{ "aria-label": text }}
                />
                {message(key)}
            </div>
        </cx>
    );

    /** An amount of money, two decimals; `name` is the input's accessible name where the label says more. */
    const money = (text: string, key: string, opts: Layout & { name?: string } = {}) => (
        <cx>
            <div>
                {label(text, opts.required)}
                <NumberField
                    value={numberValue(d[key])}
                    required={!!opts.required}
                    minValue={0}
                    format="n;2"
                    emptyText="—"
                    error={errors[key]}
                    errorTooltip={noErrorText}
                    inputAttrs={{ "aria-label": opts.name ?? text, inputMode: "decimal" }}
                />
                {message(key)}
            </div>
        </cx>
    );

    const date = (text: string, key: string, opts: Layout = {}) => (
        <cx>
            <div>
                {label(text, opts.required)}
                <DateField
                    value={dateValue(d[key])}
                    required={!!opts.required}
                    emptyText="—"
                    error={errors[key]}
                    errorTooltip={noErrorText}
                    inputAttrs={{ "aria-label": text }}
                />
                {message(key)}
            </div>
        </cx>
    );

    /** A yes/no: a checkbox while editing, words while viewing. */
    const flag = (text: string, key: string, yes: string, no: string) => (
        <cx>
            <div>
                {label(text)}
                <Checkbox visible={editing} value={d[key]} text={yes} />
                <div
                    class="editor-value"
                    visible={state.viewing}
                    text={expr(d[key], (v) => (v ? yes : no))}
                />
            </div>
        </cx>
    );

    /** The asset's own fields, the first section of every asset's page. */
    const basicInformation = () => (
        <cx>
            <section class="editor-section">
                <h2 class="editor-section-title" text="Basic information" />
                <div class="editor-grid">
                    {text("Name", "name", 300, { required: true, wide: true })}
                    {pick("Vendor", "vendor", "vendors", { required: true })}
                    {text("Invoice number", "invoiceNumber", 200)}
                    {money("Purchase value (BAM, without VAT)", "purchaseValue", {
                        required: true,
                        name: "Purchase value",
                    })}
                    {date("Purchase date", "purchaseDate", { required: true })}
                    {pick("Assignee", "person", "people", { required: true })}
                    {flag("Record", "incomplete", "Marked incomplete", "Complete")}
                    {pick("Confidentiality", "confidentiality", "confidentialities")}
                    {pick("Integrity", "integrity", "integrities")}
                    {pick("Availability", "availability", "availabilities")}
                    <div>
                        {label("Importance")}
                        <div class="editor-value" text={state.importance} />
                        <div class="editor-hint" visible={editing} text="From the three above." />
                    </div>
                    {prose("Description", "description", 1000)}
                </div>
            </section>
        </cx>
    );

    return { label, message, pick, text, prose, money, date, flag, basicInformation, editing, noErrorText };
}
