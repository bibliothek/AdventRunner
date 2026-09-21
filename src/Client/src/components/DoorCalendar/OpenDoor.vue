<template>
    <div class="door" :class="[isDone ? 'door-done' : 'door-open', { 'door-interactive': showButtonIndicator }]">
        <span class="door-panel door-frame" aria-hidden="true"></span>
        <!-- Below sm the tile is barely 60-80px, too tight for a corner badge
             next to a two-digit distance, so the day sits in the flow instead. -->
        <span
            class="mb-0.5 sm:mb-0 sm:absolute sm:top-2.5 sm:left-3 md:top-3 md:left-4
                   door-day text-[10px] sm:text-sm md:text-base"
            :class="isDone ? 'text-white/70' : 'text-primary/60'"
        >
            {{ day }}
        </span>

        <span
            v-if="isDone"
            class="absolute top-1.5 right-1.5 sm:top-2.5 sm:right-2.5 md:top-3 md:right-3 flex items-center justify-center
                   w-4 h-4 sm:w-5 sm:h-5 md:w-6 md:h-6 rounded-full bg-white/25"
        >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                class="w-2.5 h-2.5 sm:w-3 sm:h-3 md:w-4 md:h-4 stroke-white">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3.5" d="M5 13l4 4L19 7"></path>
            </svg>
        </span>

        <div class="door-distance flex flex-col items-center justify-center
                    text-[clamp(1.375rem,7.5vw,1.75rem)] sm:text-3xl md:text-4xl">
            {{ distance }}<span class="font-semibold text-[10px] tracking-wide sm:text-xs md:text-sm">km</span>
        </div>

        <div v-if="isDone" class="hidden sm:block mt-2 md:mt-3">
            <span class="text-xs md:text-sm font-semibold text-white/90">Done 🎉</span>
        </div>
        <div v-else class="hidden sm:flex mt-2 md:mt-3 items-center justify-center">
            <span v-if="showButtonIndicator" class="flex items-center gap-1.5 text-xs md:text-sm font-semibold text-primary-focus">
                Mark done
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                    class="w-4 h-4 stroke-current">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M9 5l7 7-7 7"></path>
                </svg>
            </span>
            <span v-else class="text-[10px] md:text-xs font-medium uppercase tracking-widest text-primary/50">
                To run
            </span>
        </div>
    </div>
</template>
<script lang="ts">

import { defineComponent } from "vue";

export default defineComponent({
    name: "OpenDoorComponent",
    props: {
        day: Number,
        distance: Number,
        isDone: Boolean,
        showButtonIndicator: Boolean,
    },
});

</script>
