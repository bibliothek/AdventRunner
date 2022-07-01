<template>

    <div class="antialiased sans-serif bg-gray-100 h-screen">
        <div>
            <div class="container mx-auto px-4 py-2 md:py-24">

                <div class="bg-white rounded-lg shadow overflow-hidden">

                    <div class="flex items-center justify-between py-2 px-6">
                        <div>
                            <span class="text-lg font-bold text-gray-800">December</span>
                            <span class="ml-1 text-lg text-gray-600 font-normal">202x</span>
                        </div>
                    </div>

                    <div class="-mx-1 -mb-1">
                        <div class="flex flex-wrap" style="margin-bottom: -40px;">
                            <div v-for="day in days">
                                <div style="width: 14.26%" class="px-2 py-2">
                                    <div
                                        class="text-gray-600 text-sm uppercase tracking-wide font-bold text-center">
                                        {{day}}
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="flex flex-wrap border-t border-l">
                            <div v-for="_ in blankdays">
                                <div style="width: 14.28%; height: 120px"
                                    class="text-center border-r border-b px-4 pt-2"></div>
                            </div>
                            <div v-for="(n, idx) in 31">
                                <div style="width: 14.28%; height: 120px" class="px-4 pt-2 border-r border-b relative">
                                    <div
                                        class=" text-gray-700 hover:bg-blue-200 inline-flex w-6 h-6 items-center justify-center cursor-pointer text-center leading-none rounded-full transition ease-in-out duration-100"
                                        >
                                    {{n}}
                                    </div>
                                    <div style="height: 80px;" class="overflow-y-auto mt-1">
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        </div>

    </div>
</template>
<script lang="ts">

import { defineComponent } from "vue";

export default defineComponent({
    name: "MonthlyCalendarComponent",
    data() {
        return {
            days: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
            blankdays: [],
        }
    }
});
</script>