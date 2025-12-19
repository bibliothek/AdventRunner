<template>

    <div class="flex flex-row justify-center">
        <div class="flex flex-col">
            <RunProgress :cal="cal" :year="year"></RunProgress>
            <div class="flex flex-row flex-wrap max-w-6xl justify-center">
                <div class="w-auto" v-for="door in cal.doors" :key="door.day">
                    <div v-if="door.state.Case === 'Closed'" @click="$emit('markedOpen',door)" :class="getCursorClass()">
                        <ClosedDoor :day="door.day" :showButtonIndicator="!readonly" />
                    </div>
                    <div v-if="door.state.Case === 'Open'" @click="$emit('markedDone',door)" :class="getCursorClass()">
                        <OpenDoor :day="door.day" :isDone="false" :distance="distanceFor(door)"
                            :showButtonIndicator="!readonly" />
                    </div>
                    <div @click="$emit('markedClosed',door)" :class="getCursorClass()">
                        <OpenDoor v-if="door.state.Case === 'Done'" :day="door.day" :isDone="true"
                            :showButtonIndicator="!readonly" :distance="distanceFor(door)" />
                    </div>
                </div>
            </div>
        </div>
    </div>

</template>
<script lang="ts">


import {defineComponent} from "vue";
import ClosedDoor from "./ClosedDoor.vue";
import OpenDoor from "./OpenDoor.vue";
import RunProgress from "../RunProgress.vue"
import {Calendar, Door} from "../../models/calendar";

export default defineComponent({
    name: "DoorCalendarComponent",
    components: { ClosedDoor, OpenDoor, RunProgress },
    emits: {
        markedDone: (door: Door) => true,
        markedOpen: (door: Door) => true,
        markedClosed: (door: Door) => true,
    },
    props: {
        cal: { type: Object as () => Calendar, required: true },
        readonly: { type: Boolean, required: true },
        year: { type: Number, required: true },
    },
    methods: {
        distanceFor(door: Door) {
            return door.distance * this.cal.settings.distanceFactor;
        },
        getCursorClass() {
            if(this.readonly) {
                return "cursor-default";
            }
            return "cursor-pointer";
        }
    }
});

</script>