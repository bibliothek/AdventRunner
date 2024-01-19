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
        text-center
        shadow-xl
        bg-neutral
        text-neutral-content
      ">
        <div class="text-2xl md:text-4xl " :class="paddingTopDay()">{{ day }}</div>
        <div class="hidden md:block font-bold pt-10" v-if="showButtonIndicator">
            Open
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                class="inline-block w-6 h-6 ml-2 stroke-current">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
            </svg>
        </div>
    </div>
</template>
<script lang="ts">


import { defineComponent } from "vue";

export default defineComponent({
    name: "OpenDoorComponent",
    props: {
        day: Number,
        showButtonIndicator: Boolean,
    },
    methods: {

        paddingTopDay() {
            return this.showButtonIndicator ? "pt-2 md:pt-8" : "pt-2 md:pt-14";
        }
    }
});

</script>