<template>
    <MonthlyCalendar v-if="useMonthlyCalender()" :year="year" :cal="cal" :readonly="false" @markedDone="markDone" @markedClosed="markClosed"
        @markedOpen="markOpen"></MonthlyCalendar>
    <DoorCalendar v-else :cal="cal" :readonly="false" @markedDone="markDone" @markedClosed="markClosed"
        @markedOpen="markOpen"></DoorCalendar>
</template>

<script lang="ts">
import {DisplayType, Door} from "../models/calendar";

import {defineComponent} from "@vue/runtime-core";
import MonthlyCalendar from "../components/MonthlyCalendar.vue";
import DoorCalendar from "../components/DoorCalendar/DoorCalendar.vue";
import * as actionTypes from '../store/action-types';
import {mapGetters} from "vuex";

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
        markClosed(door: Door) {
            this.$store.dispatch(actionTypes.CLOSE_DOOR, door.day)
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