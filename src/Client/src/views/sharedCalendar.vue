<template>
    <div class="flex flex-row justify-center">
        <div class="flex flex-col" v-if="hasResponse">
            <div class="text-center" v-if="displayName.length > 0">
                <div class="text-2xl font-semibold text-content">{{ displayName }}</div>
                <div class="text-md font-light">{{ sharedLinkResponse.period }}</div>
            </div>
            <DoorCalendar :cal="sharedLinkResponse.calendar" :readonly="true"></DoorCalendar>
        </div>
        <div class="h-96 flex items-center" v-else>
            <div class="flex flex-col justify-center items-center">
                <h2 class="text-4xl">404</h2>
                <p class="text-xl">Runner not found</p>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { Door } from "../models/calendar";

import { defineComponent } from "@vue/runtime-core";
import DoorCalendar from "../components/DoorCalendar/DoorCalendar.vue";
import ClosedDoor from "../components/DoorCalendar/ClosedDoor.vue";
import OpenDoor from "../components/DoorCalendar/OpenDoor.vue";
import RunProgress from "../components/RunProgress.vue";
import { getSome, isSome } from "../models/fsharp-helpers";
import * as actionTypes from "../store/action-types"
import { sharedCalendarRoute } from "../router/router";

export default defineComponent({
    name: "SharedCalendarComponent",
    components: { ClosedDoor, OpenDoor, RunProgress, DoorCalendar },
    computed: {
        hasResponse() {
            return isSome(this.$store.state.currentSharedCalendar[1]);
        },
        sharedLinkResponse() {
            return getSome(this.$store.state.currentSharedCalendar[1]);
        },
        displayName() {
            const sharedCalendar = getSome(this.$store.state.currentSharedCalendar[1]);
            return isSome(sharedCalendar.displayName) ? getSome(sharedCalendar.displayName) : "";
        }
    },
    methods: {
        distanceFor(door: Door) {
            if (this.hasResponse) {
                return door.distance * this.sharedLinkResponse.calendar.settings.distanceFactor;
            }
            return 0;
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