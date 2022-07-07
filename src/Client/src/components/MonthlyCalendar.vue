<template>

    <div class="flex justify-center">
        <div class="bg-white max-w-6xl rounded-lg shadow overflow-hidden">

            <div class="flex items-center justify-between py-2 px-6">
                <div>
                    <span class="text-lg font-bold text-gray-800">December</span>
                    <span class="ml-1 text-lg text-gray-600 font-normal">{{ year }}</span>
                </div>
            </div>

            <div class="-mx-1 -mb-1">
                <div class="flex flex-wrap border-t">

                    <div v-for="day in days" style="width: 14.26%" class="px-2 py-2 border-l border-r">
                        <div class="text-gray-600 text-sm uppercase tracking-wide font-bold text-center">
                            {{ day }}
                        </div>
                    </div>

                </div>

                <div class="flex flex-wrap border-t border-l">
                    <div v-for="_ in blankdays" style="width: 14.28%; height: 120px"
                        class="text-center border-r border-b px-4 pt-2"></div>
                    <div v-for="door in cal?.doors" style="width: 14.28%; height: 120px"
                        class="px-4 pt-2 border-r border-b rounded-sm relative" :class="getDoorClasses(door)">
                        <div
                            class="h-6 day-indicator items-center justify-center" :class="getDayIndicatorClasses(door)">
                            {{ door.day }}
                        </div>
                        <div style="height: 100%"
                            class=" -mt-7 flex flex-col justify-center items-center text-center text-sm lg:text-2xl lg:font-light"
                            :class="getDayContentClasses(door)">
                            <div v-if="door.state.case !== 'Closed'" class="mb-2 mt-4">{{ door.distance * cal.settings.distanceFactor }} km</div>

                            <font-awesome-icon icon="fa-solid fa-check" v-if="door.state.case === 'Done'"/>
                            <font-awesome-icon icon="fa-solid fa-person-running" v-if="door.state.case === 'Open'"/>
                            <font-awesome-icon icon="fa-solid fa-door-open" v-if="door.state.case === 'Closed'"/>
                        </div>
                    </div>
                    <div v-for="(n, _) in [...Array(7).keys()].map(v => v + 25)" style="width: 14.28%; height: 120px"
                        class="px-4 pt-2 border-r border-b relative text-gray-500">
                        <div
                            class="h-6 items-center justify-center day-indicator leading-none rounded-full transition ease-in-out duration-100">
                            {{ n }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="postcss">
    .day-indicator {
        @apply text-center lg:text-base text-xs
    }
</style>

<script lang="ts">

import { defineComponent } from "vue";
import ClosedDoor from "../components/ClosedDoor.vue";
import { Calendar, Door, DoorStateCase } from "../models/calendar";

export default defineComponent({
    name: "MonthlyCalendarComponent",
    components: { ClosedDoor },
    data() {
        return {
            days: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
        }
    },
    props: {
        cal: { type: Object as () => Calendar, required: true },
        year: { type: Number, required: true },
        readonly: {type: Boolean, default: false},
    },
    computed: {
        blankdays() {
            const firstDayOfMonth = (new Date(this.year, 11)).getDay();
            const blankDaysArray = [];
            for (var i = 1; i < firstDayOfMonth; i++) {
                blankDaysArray.push(i);
            }
            return blankDaysArray;
        },
    },
    methods: {
        getDoorClasses(door: Door) {

            let selectColor = (c: DoorStateCase): string => {

            switch (c) {
                case "Closed":
                    return "bg-neutral";
                case "Open":
                    return "bg-gray-100";
                case "Done":
                    return "bg-primary";
                default:
                    return "";

            }
            }
            const cursor = this.readonly ? 'cursor-default' : 'cursor-pointer';
            const color = selectColor(door.state.case);
            return `${cursor} ${color}`;
        },
        getDayContentClasses(door: Door) {
            switch (door.state.case) {
                case "Closed":
                    return "text-neutral-content";
                case "Open":
                    return "";
                case "Done":
                    return "text-primary-content";
                default:
                    return "";

            }
        },
        getDayIndicatorClasses(door: Door) {
            switch (door.state.case) {
                case "Closed":
                    return "text-gray-100";
                case "Open":
                    return "text-gray-500";
                case "Done":
                    return "text-gray-100";
                default:
                    return "";

            }
        }
    }

});
</script>