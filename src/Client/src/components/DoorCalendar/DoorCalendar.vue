<template>
    <div class="w-full max-w-5xl mx-auto">
        <RunProgress :cal="cal" :year="year"></RunProgress>

        <div class="grid grid-cols-4 lg:grid-cols-6 gap-2 sm:gap-3 lg:gap-5">
            <component
                :is="readonly ? 'div' : 'button'"
                v-for="door in cal.doors"
                :key="door.day"
                :type="readonly ? undefined : 'button'"
                :aria-label="ariaLabelFor(door)"
                class="block w-full appearance-none bg-transparent border-0 p-0 rounded-2xl md:rounded-3xl
                       focus:outline-none focus-visible:ring-4 focus-visible:ring-primary/50 focus-visible:ring-offset-2"
                :class="readonly ? 'cursor-default' : 'cursor-pointer'"
                @click="doorClicked(door)"
            >
                <ClosedDoor
                    v-if="door.state.Case === 'Closed'"
                    :day="door.day"
                    :showButtonIndicator="!readonly"
                />
                <OpenDoor
                    v-else
                    :day="door.day"
                    :isDone="door.state.Case === 'Done'"
                    :distance="distanceFor(door)"
                    :showButtonIndicator="!readonly"
                />
            </component>
        </div>

        <p v-if="!readonly" id="door-hint" class="mt-4 md:mt-6 text-center text-xs md:text-sm text-ink-400">
            Tap a door to open it, tap again once you've run the distance.
        </p>
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
            return Math.round(door.distance * this.cal.settings.distanceFactor * 10) / 10;
        },
        ariaLabelFor(door: Door) {
            switch (door.state.Case) {
                case "Closed":
                    return `Open door ${door.day}`;
                case "Open":
                    return `Door ${door.day}, ${this.distanceFor(door)} km to run. Mark as done`;
                case "Done":
                    return `Door ${door.day}, ${this.distanceFor(door)} km done. Close again`;
                default:
                    return `Door ${door.day}`;
            }
        },
        doorClicked(door: Door) {
            if (this.readonly) {
                return;
            }
            switch (door.state.Case) {
                case "Closed":
                    this.$emit('markedOpen', door);
                    return;
                case "Open":
                    this.$emit('markedDone', door);
                    return;
                case "Done":
                    this.$emit('markedClosed', door);
                    return;
            }
        }
    }
});

</script>
