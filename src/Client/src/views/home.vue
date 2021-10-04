<template>
    
</template>

<script lang="js">
import {inject} from 'vue';
export default {
  data() {
    return { };
  },
  inject: ["Auth"],
  setup() {
    const auth = inject("Auth");
    return {
      ...auth,
    };
  },
  mounted() {
    if(this.isAuthenticated) {
        this.$router.push("calendar");
    } else {
        this.$router.push("welcome");
    }
  }
};
</script>