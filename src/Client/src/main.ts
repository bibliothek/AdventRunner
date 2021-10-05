import { createApp } from "vue";

import App from "./App.vue";
import "./index.css";
import router from "./router/router";
import {Auth0} from "./auth";

async function init() {
    const AuthPlugin = await Auth0.init({
        onRedirectCallback: (appState) => {
            router.push(
                appState && appState.targetUrl
                    ? appState.targetUrl
                    : window.location.pathname,
            )
        },
        clientId: 'mQrnkRPrBYmNqKy84eHjWu8od7orUR9F',
        domain: 'adventrunner.eu.auth0.com',
        audience: 'AdventRunner',
        redirectUri: window.location.origin + '/#/calendar',
    });
    const app = createApp(App);
    app
        .use(AuthPlugin)
        .use(router)
        .mount('#app');
}

init();
