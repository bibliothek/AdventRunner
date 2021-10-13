<template>
  <div class="flex-row flex items-baseline mb-4">
    <h1 class="text-4xl md:text-5xl text-primary" @click="goToCalendar">Adventrunner</h1>
    <div class="flex-grow"></div>
      <div class="dropdown dropdown-end mr-3" v-if="isAuthenticated">
      <div tabindex="0" class="cursor-pointer">
        <img
          class="
            rounded-full
            w-10
            h-10
            border-white
            hover:border-gray-300
            border-4
          "
          :src="user.picture"
        />
      </div>
      <ul
        tabindex="0"
        class="p-2 shadow menu dropdown-content bg-base-100 rounded-box w-36"
      >
        <li>
          <a @click="goToSettings">Settings</a>
        </li>
        <li>
          <a @click="logout">Log out</a>
        </li>
      </ul>
    </div>
  </div>
</template>

<script lang="js">
import {inject} from 'vue';
export default {
  data() {
    return { };
  },
  inject: ["Auth"],
  methods: {
      logout() {
        this.Auth.logout();
      },
      goToCalendar() {
        this.$router.push({ path: 'calendar' });
      },
      goToSettings() {
        this.$router.push({ path: 'settings' });
      }
  },
  setup() {
    const auth = inject("Auth");
    return {
      ...auth,
    };
  },
};
</script>