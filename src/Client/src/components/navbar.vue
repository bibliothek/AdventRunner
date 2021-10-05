<template>
  <div class="flex-row flex items-baseline mb-4">
    <h1 class="text-5xl text-primary">Adventrunner</h1>
    <div class="flex-grow"></div>
    <div class="dropdown dropdown-end mr-3" v-if="isAuthenticated">
      <div tabindex="0" class="cursor-pointer">
        <img
          class="
            rounded-full
            w-auto
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
        const options = {returnTo: window.location.origin};
        this.Auth.logout(options);
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