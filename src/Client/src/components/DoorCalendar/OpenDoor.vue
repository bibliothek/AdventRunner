<template>
    <div class="
      m-2
      pt-1
      md:m-4
      w-14
      h-14
      md:w-40
      md:h-40
      rounded-box
      text-center text-gray-500
    " :class="getCardStyle()">
        <div class="hidden md:block md:text-4xl md:pt-8" :class="getTextStyle()">{{ day }}</div>
        <div class="text-sm md:text-2xl pt-2 font-bold md:pt-2" :class="getTextStyle()">
            {{ distance }} km
        </div>
        <div class="md:hidden pt-0 text-xs" v-if="isDone">🎉</div>
        <div class="md:hidden pt-0.5 text-xs flex flex-row" v-else>
            <div class="flex-grow"></div>
            <img class="w-3" src="../../../public/runner.png" alt="Done" />
            <div class="flex-grow"></div>
        </div>
        <div class="hidden md:block">
            <div class="font-bold text-primary-focus pt-2" v-if="!isDone && showButtonIndicator">
                Done
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                    class="inline-block w-6 h-6 ml-2 stroke-current">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
                </svg>
            </div>
            <div class="font text-primary-content pt-2" v-if="isDone">Done🎉</div>
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
    methods: {
        getCardStyle(): string {
            if (this.isDone!) {
                return "bg-primary shadow-md";
            }
            return "bg-gray-100 shadow-xl";
        },

        getTextStyle(): string {
            if (this.isDone) {
                return "text-neutral-content";
            }
            return "text-primary";
        }
    }
});

</script>