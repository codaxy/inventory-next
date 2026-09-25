import { equal, FirstVisibleChildLayout, falsy } from "cx/ui";
import { PureContainer, RedirectRoute, Route } from "cx/widgets";

import $app from "../model";
import Controller from "./Controller";
import Home from "./home";
import SignIn from "./sign-in";

// The first matching route wins, so order is the routing table: signed in or not is the outermost
// split, and everything below it can assume the answer.
export default (
    <cx>
        <PureContainer layout={FirstVisibleChildLayout} controller={Controller}>
            <PureContainer visible={equal($app.session.status, "loading")}>
                <div class="page">
                    <p text="Loading…" />
                </div>
            </PureContainer>

            <PureContainer layout={FirstVisibleChildLayout} if={falsy($app.session.user)}>
                <Route route="~/sign-in" url={$app.url}>
                    <SignIn />
                </Route>
                <RedirectRoute route="*any" url={$app.url} redirect="~/sign-in" />
            </PureContainer>

            <PureContainer layout={FirstVisibleChildLayout}>
                <Route route="~/" url={$app.url}>
                    <Home />
                </Route>
                <RedirectRoute route="*any" url={$app.url} redirect="~/" />
            </PureContainer>
        </PureContainer>
    </cx>
);
