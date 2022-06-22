import { createRouter, createWebHashHistory } from "vue-router";
import { Auth0 } from "../auth";
import calendar from "../views/calendar.vue";
import sharedCalendar from "../views/sharedCalendar.vue"
import settings from "../views/settings.vue";
import welcome from "../views/welcome.vue";
import home from "../views/home.vue";

const routes = [
    { path: "/calendar", component: calendar, beforeEnter: Auth0.routeGuard },
    { path: "/settings", component: settings, beforeEnter: Auth0.routeGuard },
    { path: "/welcome", component: welcome },
    { path: "/s/:id", component: sharedCalendar},
    { path: "/", component: home },
];

const router = createRouter({
    history: createWebHashHistory(),
    routes,
});

export default router;
