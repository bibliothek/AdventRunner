<template>
    <div class="min-h-screen">
        <div class="container mx-auto px-2 py-3 sm:px-4 sm:py-6">
            <div class="bg-white rounded-2xl sm:rounded-3xl shadow-soft overflow-hidden flex flex-col">
                <Navbar></Navbar>
                <div class="w-full h-96 flex flex-col gap-3 items-center justify-center text-ink-400" v-if="loading">
                    <font-awesome-icon icon="fa-solid fa-spinner" class="fa-2xl animate-spin text-primary"/>
                    <span class="text-sm font-medium">Unwrapping your calendar…</span>
                </div>
                <div class="px-3 pb-5 sm:px-6 sm:pb-8" v-else>
                    <router-view></router-view>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import Navbar from './navbar.vue';
export default defineComponent({
    name: "ShellComponent",
    components: { Navbar },
    computed: {
        loading() {
            return this.$store.state.loading;
        }
    }

});
</script>
