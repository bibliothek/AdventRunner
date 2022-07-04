<template>

<div class="flex justify-center">
    <div class="bg-white max-w-6xl rounded-lg shadow overflow-hidden">

        <div class="flex items-center justify-between py-2 px-6">
            <div>
                <span class="text-lg font-bold text-gray-800">December</span>
                <span class="ml-1 text-lg text-gray-600 font-normal">{{year}}</span>
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
                <div v-for="(n, idx) in 31" style="width: 14.28%; height: 120px"
                    class="px-4 pt-2 border-r border-b relative bg-neutral text-neutral-content">
                    <div
                        class="inline-flex w-6 h-6 items-center justify-center cursor-pointer text-center leading-none rounded-full transition ease-in-out duration-100">
                        {{ n }}
                    </div>
                    <div style="height: 80px;" class="overflow-y-auto mt-1">


                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
</template>
<script lang="ts">

import { defineComponent } from "vue";
import ClosedDoor from "../components/ClosedDoor.vue";
import OpenDoor from "../components/OpenDoor.vue";
import { Calendar } from "../models/calendar";

export default defineComponent({
    name: "MonthlyCalendarComponent",
    components: {ClosedDoor},
    data() {
        return {
            days: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
        }
    },
    props: {
        cal: Object as () => Calendar,
        year: {type: Number, required: true}
    },
    computed: {
        blankdays() {
            const firstDayOfMonth = (new Date(this.year, 11)).getDay();
            console.log(new Date(this.year, 11))
            console.log(firstDayOfMonth)
            const blankDaysArray = [];
            for ( var i=1; i < firstDayOfMonth; i++) {
						blankDaysArray.push(i);
					}
            return blankDaysArray;
        },
    }

});
</script>