<template>

    <div class="flex flex-row justify-center">
        <div class="flex flex-col">
            <RunProgress :cal="cal"></RunProgress>
            <div class="flex flex-row flex-wrap max-w-6xl justify-center">
                <div class="w-auto" v-for="door in cal.doors" :key="door.day">
                    <button v-if="door.state.case === 'Closed'" @click="$emit('markedOpen',{door})">
                        <ClosedDoor :day="door.day" :showButtonIndicator="!readonly" />
                    </button>
                    <button v-if="door.state.case === 'Open'" @click="$emit('markedDone',{door})">
                        <OpenDoor :day="door.day" :isDone="false" :distance="distanceFor(door)"
                            :showButtonIndicator="!readonly" />
                    </button>
                    <button @click="$emit('markedOpen',{door})">
                        <OpenDoor v-if="door.state.case === 'Done'" :day="door.day" :isDone="true"
                            :showButtonIndicator="!readonly" :distance="distanceFor(door)" />
                    </button>
                </div>
            </div>
        </div>
    </div>

</template>
<script lang="ts">


import { defineComponent } from "vue";
import ClosedDoor from "./ClosedDoor.vue";
import OpenDoor from "./OpenDoor.vue";
import { Calendar, Door, DoorStateCase } from "../../models/calendar";

export default defineComponent({
    name: "DoorCalendarComponent",
    components: { ClosedDoor, OpenDoor },
    emits: {
        markedDone(payload: { door: Door }) {
            return true;
        },
        markedOpen(payload: { door: Door }) {
            return true;
        }
    },
    props: {
        cal: { type: Object as () => Calendar, required: true },
        readonly: { type: Boolean, required: true },
    },
    methods: {
        distanceFor(door: Door) {
            return door.distance * this.$store.getters.displayCalendar.settings.distanceFactor;
        },
    }
});

</script>