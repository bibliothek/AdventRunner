<template>
  <div>
    <p class="text-neutral">{{ apiMessage }}</p>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "external-api",
  inject: ["Auth"],
  data() {
    return {
      apiMessage: ""
    };
  },
  mounted() {
    this.callApi();
  },
  methods: {
    async callApi() {
      // Get the access token from the auth wrapper
      const token = await this.Auth.getTokenSilently();

      // Use Axios to make a call to the API
      const { data } = await axios.get("/api/calendars", {
        headers: {
          Authorization: `Bearer ${token}`    // send the access token through the 'Authorization' header
        }
      });

      this.apiMessage = data;
    }
  }
};
</script>