<template>
    <MonthlyCalendar v-if="useMonthlyCalender()" :year="year" :cal="cal" :readonly="false" @markedDone="(door) => markDone(door)"
        @markedOpen="(door) => markOpen(door)"></MonthlyCalendar>
    <DoorCalendar v-else :cal="cal" :readonly="false" @markedDone="(payload) => markDone(payload.door)"
        @markedOpen="(payload) => markOpen(payload.door)"></DoorCalendar>
</template>

<script lang="ts">
import { Door, DisplayType } from "../models/calendar";

import { defineComponent } from "@vue/runtime-core";
import MonthlyCalendar from "../components/MonthlyCalendar.vue";
import DoorCalendar from "../components/DoorCalendar/DoorCalendar.vue";
import * as actionTypes from '../store/action-types';
import { mapGetters } from "vuex";

export default defineComponent({
    name: "CalendarComponent",
    components: { MonthlyCalendar, DoorCalendar },
    computed: {
        ...mapGetters({
            cal: "displayCalendar",
            year: "displayPeriod",
            displayType: "displayType",
        }),
    },
    methods: {
        markDone(door: Door) {
            this.$store.dispatch(actionTypes.MARK_DOOR_DONE, door.day)
        },
        markOpen(door: Door) {
            this.$store.dispatch(actionTypes.OPEN_DOOR, door.day)
        },
        useMonthlyCalender() {
            return this.displayType === DisplayType.Monthly;
        },
    },
    mounted() {
        this.$store.dispatch(actionTypes.GET_CALENDAR)
    }
})

</script>