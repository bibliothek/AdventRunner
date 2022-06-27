<template>
    <div class="flex flex-row">
        <div class="flex-grow"></div>
        <div class="flex flex-col">
            <div class="mb-4 text-center" v-if="displayName.length > 0">
                <div class="text-2xl font-semibold text-content">{{ displayName }}</div>
                <div class="text-md font-light">{{ sharedLinkResponse.period }}</div>
            </div>
            <div class="flex-grow-1"></div>
            <div class="flex flex-row flex-wrap max-w-6xl justify-center">
                <div class="w-auto" v-for="door in sharedLinkResponse.calendar.doors" :key="door.day">
                    <ClosedDoor v-if="door.state.case === 'Closed'" :day="door.day" />
                    <OpenDoor v-if="door.state.case === 'Open'" :day="door.day" :isDone="false"
                        :distance="distanceFor(door)" />
                    <OpenDoor v-if="door.state.case === 'Done'" :day="door.day" :isDone="true"
                        :distance="distanceFor(door)" />
                </div>
            </div>
        </div>
        <div class="flex-grow"></div>
    </div>
</template>

<script lang="ts">
import { Door } from "../models/calendar";

import { defineComponent } from "@vue/runtime-core";
import ClosedDoor from "../components/ClosedDoor.vue";
import OpenDoor from "../components/OpenDoor.vue";
import { getSome, isSome } from "../models/fsharp-helpers";
import * as actionTypes from "../store/action-types"
import { sharedCalendarRoute } from "../router/router";

export default defineComponent({
    name: "SharedCalendarComponent",
    components: { ClosedDoor, OpenDoor },
    computed: {
        sharedLinkResponse() {
            return this.$store.state.currentSharedCalendar[1];
        },
        displayName() {
            const sharedCalendar = this.$store.state.currentSharedCalendar[1];
            return isSome(sharedCalendar.displayName) ? getSome(sharedCalendar.displayName) : "";
        }
    },
    methods: {
        distanceFor(door: Door) {
            return door.distance * this.sharedLinkResponse.calendar.settings.distanceFactor;
        },
        getCal() {
            const id = this.$route.params['id'];
            this.$store.dispatch(actionTypes.GET_SHARED_CALENDAR, id);
        }
    },
    mounted() {
        this.getCal();
    },
    watch: {
        $route(to, from) {
            if (to !== from && to.name === sharedCalendarRoute) {
                this.getCal();
            }
        }
    }
})

</script>