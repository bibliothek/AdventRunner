<template>
    <MonthlyCalendar :year="year" :cal="cal" :readonly="false" @markedDone="(door) => markDone(door)"
        @markedOpen="(door) => markOpen(door)"></MonthlyCalendar>
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