<template>
    <div class="container m-auto p-4 bg-gray-100">
        <div class="bg-white p-4 flex flex-col">
            <Navbar></Navbar>
            <div class="w-full h-96 flex items-center justify-center" v-if="loading">
                <font-awesome-icon icon="fa-solid fa-spinner" class="fa-2xl animate-spin"/>
            </div>
            <router-view v-else></router-view>
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