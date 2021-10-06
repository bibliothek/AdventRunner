<script lang="js">
import {inject} from 'vue';
export default {
  data() {
    return { };
  },
  inject: ["Auth"],
  methods: {
    login() {
      this.Auth.loginWithRedirect();
    },
    logout() {
        this.Auth.logout();
    },
    goToCalendar() {
      this.$router.push("calendar")
    },
  },
  

  setup() {
    const auth = inject("Auth");
    return {
      ...auth,
    };
  },
};
</script>
<template>
  <div class="p-6 md:flex md:flex-row-reverse">
    <div class="flex flex-col w-full md:w-1/2 items-center pb-3">
      <div class="flex-grow"></div>
      <div v-if="!loading">
        <div v-if="!isAuthenticated">
          <button class="btn btn-primary btn-md" @click="login">Log in</button>
        </div>
        <div class="flex flex-col" v-if="isAuthenticated">
          <button class="btn btn-primary btn-md mb-2" @click="goToCalendar">
            Go to calendar
          </button>
          <button class="btn btn-outline btn-secondary btn-md" @click="logout">
            Log out
          </button>
        </div>
      </div>
      <div class="flex-grow"></div>
    </div>
    <div class="flex-shrink md:w-1/2 flex flex-col items-center">
      <div class="bg-white text-primary font-bold p-3 rounded w-full">
        <p class="text-center cursor-default">
          Open a door and get out running!
        </p>
      </div>
      <img class="mt-3 opacity-90" src="../assets/preview.png" alt="preview" />
    </div>
  </div>
</template>