import { createApp } from "vue";

import App from "./App.vue";
import "./index.css";
import router from "./router/router";
import {Auth0} from "./auth";
import {
    store
} from "./store";

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCopy, faSpinner } from '@fortawesome/free-solid-svg-icons'
import { faCheckSquare } from '@fortawesome/free-regular-svg-icons'

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

    library.add(faCopy)
    library.add(faSpinner)
    library.add(faCheckSquare)

    const app = createApp(App);
    app
        .component('font-awesome-icon', FontAwesomeIcon)
        .use(AuthPlugin)
        .use(router)
        .use(store)
        .mount('#app');
}

init();
