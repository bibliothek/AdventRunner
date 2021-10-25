<template>
    <div class="flex flex-row">
        <div class="flex-grow"></div>
        <div class="flex flex-row flex-wrap max-w-6xl justify-center">
            <div class="w-auto" v-for="door in calendar.doors" :key="door.day">
                <ClosedDoor
                    v-if="door.state.case === 'Closed'"
                    :day="door.day"
                    @opened="markOpen(door)"
                />
                <button v-if="door.state.case === 'Open'" @click="markDone(door)">
                    <OpenDoor :day="door.day" :isDone="false" :distance="distanceFor(door)" />
                </button>
                <button @click="markOpen(door)">
                    <OpenDoor
                        v-if="door.state.case === 'Done'"
                        :day="door.day"
                        :isDone="true"
                        :distance="distanceFor(door)"
                    />
                </button>
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
import * as actionTypes from '../store/action-types';
export default defineComponent({
    name: "CalendarComponent",
    computed: {
        calendar() {
            return this.$store.state.calendar;
        }
    },
    components: { ClosedDoor, OpenDoor },
    methods: {
        distanceFor(door: Door) {
            return door.distance * this.$store.state.calendar.settings.distanceFactor;
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