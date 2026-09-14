<template>
    <div class="door door-closed" :class="{ 'door-interactive': showButtonIndicator }">
        <span class="door-panel door-panel-l" aria-hidden="true"></span>
        <span class="door-panel door-panel-r" aria-hidden="true"></span>
        <div class="door-day text-3xl sm:text-5xl md:text-6xl">{{ day }}</div>
    </div>
</template>
<script lang="ts">


import { defineComponent } from "vue";

export default defineComponent({
    name: "ClosedDoorComponent",
    props: {
        day: Number,
        showButtonIndicator: Boolean,
    },
});

</script>
