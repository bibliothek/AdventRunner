<template>
    <div class="navbar bg-base-100">
        <div class="flex-1">
            <a class="btn btn-ghost normal-case text-primary text-2xl" @click="goToCalendar">Adventrunner</a>
        </div>
        <div class="flex-none" v-if="isAuthenticated">
            <div class="dropdown dropdown-end">
                <label tabindex="0" class="btn btn-ghost normal-case">{{ displayPeriod }}</label>
                <ul tabindex="0" class="menu  dropdown-content mt-3 p-2 shadow bg-base-100 rounded-box">
                    <li v-for="period in periods"><a @click="setDisplayPeriod(period)">{{ period }}<span
                                v-if="period == displayPeriod" class="text-xs text-neutral">&nbsp;(current)</span></a></li>
                </ul>
            </div>
            <div class="dropdown dropdown-end">
                <label tabindex="0" class="btn btn-ghost btn-circle avatar">
                    <div class="w-10 rounded-full">
                        <img :src="user.picture" />
                    </div>
                </label>
                <ul tabindex="0" class="menu  dropdown-content mt-3 p-2 shadow bg-base-100 rounded-box">
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
export default {
    data() {
        return {};
    },
    computed: {
        ...mapGetters(['periods', 'displayPeriod']
        ),
    },
    inject: ["Auth"],
    methods: {
        logout() {
            this.Auth.logout();
        },
        goToCalendar() {
            this.$router.push({ path: 'calendar' });
        },
        goToSettings() {
            this.$router.push({ path: 'settings' });
        },
        setDisplayPeriod(period) {
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