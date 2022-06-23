<template>
    <div class="flex flex-row">
        <div class="flex-grow"></div>
        <div class="flex flex-row flex-wrap max-w-6xl justify-center">
            <div class="w-auto" v-for="door in cal.doors" :key="door.day">
                <ClosedDoor v-if="door.state.case === 'Closed'" :day="door.day" />
                <OpenDoor v-if="door.state.case === 'Open'" :day="door.day" :isDone="false"
                    :distance="distanceFor(door)" />
                <OpenDoor v-if="door.state.case === 'Done'" :day="door.day" :isDone="true"
                    :distance="distanceFor(door)" />
            </div>
        </div>
        <div class="flex-grow"></div>
    </div>
</template>

<script lang="ts">
import { Calendar, Door, emptyCalendar } from "../models/calendar";

import { defineComponent } from "@vue/runtime-core";
import ClosedDoor from "../components/ClosedDoor.vue";
import OpenDoor from "../components/OpenDoor.vue";
import axios from "axios";

export default defineComponent({
    name: "SharedCalendarComponent",
    components: { ClosedDoor, OpenDoor },
    data() {
        return {
            cal: emptyCalendar()
        };
    },
    methods: {
        distanceFor(door: Door) {
            return door.distance * this.cal.settings.distanceFactor;
        },
    },
    async mounted() {
        const id = this.$route.params['id'];
        const response = await axios.get<Calendar>("/api/sharedCalendars/" + id);
        this.cal = response.data;
    }
})

</script>