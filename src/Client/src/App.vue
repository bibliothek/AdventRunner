<script setup lang="ts">
import Navbar from './components/navbar.vue';
// This starter template is using Vue 3 <script setup> SFCs
// Check out https://v3.vuejs.org/api/sfc-script-setup.html#sfc-script-setup
</script>

<template>
  <div class="container m-auto p-4 bg-gray-100">
    <div class="bg-white p-4 flex flex-col">
      <Navbar></Navbar>
      <router-view></router-view>
    </div>
  </div>
</template>

