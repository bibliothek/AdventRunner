<template>
    <!-- <div class="flex flex-row justify-center">
        <div class="flex flex-col">
            <RunProgress :cal="cal"></RunProgress>
            <div class="flex flex-row flex-wrap max-w-6xl justify-center">
                <div class="w-auto" v-for="door in cal.doors" :key="door.day">
                    <button v-if="door.state.case === 'Closed'" @click="markOpen(door)">
                        <ClosedDoor :day="door.day" :showButtonIndicator="true" />
                    </button>
                    <button v-if="door.state.case === 'Open'" @click="markDone(door)">
                        <OpenDoor :day="door.day" :isDone="false" :distance="distanceFor(door)"
                            :showButtonIndicator="true" />
                    </button>
                    <button @click="markOpen(door)">
                        <OpenDoor v-if="door.state.case === 'Done'" :day="door.day" :isDone="true"
                            :showButtonIndicator="true" :distance="distanceFor(door)" />
                    </button>
                </div>
            </div>
        </div>
    </div> -->
    <MonthlyCalendar :year="year" :cal="cal"></MonthlyCalendar>
</template>

<script lang="ts">
import { Door } from "../models/calendar";

import { defineComponent } from "@vue/runtime-core";
import ClosedDoor from "../components/ClosedDoor.vue";
import OpenDoor from "../components/OpenDoor.vue";
import MonthlyCalendar from "../components/MonthlyCalendar.vue";
import RunProgress from "../components/RunProgress.vue";
import * as actionTypes from '../store/action-types';
import { mapGetters } from "vuex";

export default defineComponent({
    name: "CalendarComponent",
    components: { ClosedDoor, OpenDoor, RunProgress, MonthlyCalendar },
    computed: {
        ...mapGetters({
            cal: "displayCalendar",
            year: "displayPeriod",

        }),
    },
    methods: {
        distanceFor(door: Door) {
            return door.distance * this.$store.getters.displayCalendar.settings.distanceFactor;
        },
        markDone(door: Door) {
            this.$store.dispatch(actionTypes.MARK_DOOR_DONE, door.day)
        },
        markOpen(door: Door) {
            this.$store.dispatch(actionTypes.OPEN_DOOR, door.day)
        }
    },
    mounted() {
        this.$store.dispatch(actionTypes.GET_CALENDAR)
    }
})

</script>