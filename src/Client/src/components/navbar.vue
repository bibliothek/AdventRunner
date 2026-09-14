<template>
    <div id="navbar" class="navbar bg-base-100 min-h-0 px-2 py-2 sm:px-4 sm:py-3 border-b border-ink-100">
        <div class="flex-1 min-w-0">
            <a class="btn btn-ghost normal-case gap-2 px-2 sm:px-3" @click="goToCalendar">
                <span
                    class="w-8 h-8 sm:w-9 sm:h-9 rounded-xl bg-primary text-primary-content flex items-center justify-center shadow-door shrink-0">
                    <font-awesome-icon icon="fa-solid fa-person-running" class="text-base sm:text-lg" />
                </span>
                <span class="text-primary text-xl sm:text-2xl font-bold tracking-tight truncate">Adventrunner</span>
            </a>
        </div>
        <div class="flex-none flex items-center gap-1" v-if="isAuthenticated">
            <div class="dropdown dropdown-end" v-if="!isSharedCalendarView">
                <label tabindex="0" class="btn btn-ghost btn-sm sm:btn-md normal-case font-semibold tnum">
                    {{ displayPeriod }}
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                        class="w-4 h-4 ml-1 stroke-current opacity-60">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
                    </svg>
                </label>
                <ul tabindex="0" class="menu dropdown-content mt-3 p-2 shadow-soft bg-base-100 rounded-2xl border border-ink-100 z-30">
                    <li v-for="period in periods" :key="period"><a @click="setDisplayPeriod(period)">{{ period }}<span
                                v-if="period == latestPeriod" class="text-xs text-ink-400">&nbsp;(current)</span></a>
                    </li>
                </ul>
            </div>
            <div class="dropdown dropdown-end">
                <label tabindex="0" class="btn btn-ghost btn-circle avatar">
                    <div class="w-9 sm:w-10 rounded-full ring-2 ring-primary/30">
                        <img :src="user.picture" alt="" />
                    </div>
                </label>
                <ul tabindex="0" class="menu dropdown-content mt-3 p-2 shadow-soft bg-base-100 rounded-2xl border border-ink-100 z-30">
                    <li><a @click="goToSettings">Settings</a></li>
                    <li><a @click="logout">Logout</a></li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script lang="js">
import { inject } from 'vue';
import * as actionTypes from '../store/action-types';
import { mapGetters } from "vuex";
import { sharedCalendarRoute } from '../router/router'
export default {
    data() {
        return {};
    },
    computed: {
        ...mapGetters(['periods', 'displayPeriod', 'latestPeriod']
        ),
        isSharedCalendarView() {
            return this.$route.name === sharedCalendarRoute;
        },
    },
    inject: ["Auth"],
    methods: {
        logout() {
            document.activeElement.blur();
            this.Auth.logout();
        },
        goToCalendar() {
            this.$router.push({ path: '/calendar' });
        },
        goToSettings() {
            document.activeElement.blur();
            this.$router.push({ path: '/settings' });
        },
        setDisplayPeriod(period) {
            document.activeElement.blur();
            this.$store.dispatch(actionTypes.SET_DISPLAY_PERIOD, period);
        }
    },
    setup() {
        const auth = inject("Auth");
        return {
            ...auth,
        };
    },
};
</script>
