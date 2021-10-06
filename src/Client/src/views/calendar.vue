<template>
  <div class="flex flex-row">
    <div class="flex-grow"></div>
    <div class="flex flex-row flex-wrap max-w-6xl justify-center">
      <div class="w-auto" v-for="door in calendar.doors" :key="door.day">
        <ClosedDoor
          v-if="door.state.case === 'Closed'"
          :day="door.day"
          @opened="markOpen(door.day)"
        />
        <button v-if="door.state.case === 'Open'" @click="markDone(door.day)">
          <OpenDoor :day="door.day" :isDone="false" :distance="door.distance" />
        </button>
        <button class="cursor-default">
          <OpenDoor
            v-if="door.state.case === 'Done'"
            :day="door.day"
            :isDone="true"
            :distance="door.distance"
          />
        </button>
      </div>
    </div>
    <div class="flex-grow"></div>
  </div>
</template>

<script lang="ts">
import axios from "axios";
import { Calendar, emptyCalendar } from "../models/calendar";

import { Options, Vue } from "vue-class-component";
import { inject } from "@vue/runtime-core";
import ClosedDoor from "../components/ClosedDoor.vue";
import OpenDoor from "../components/OpenDoor.vue";
@Options({
  components: {
    ClosedDoor,
    OpenDoor,
  },
})
export default class CalendarComponent extends Vue {
  calendar = emptyCalendar();

  private Auth = inject("Auth") as any;

  private async getAxiosConfig(): Promise<any> {
    const token = await this.Auth.getTokenSilently();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    };
  }

  async markDone(day: number) {
    this.calendar = {
      ...this.calendar,
      doors: this.calendar.doors.map((door) =>
        door.day === day ? { ...door, state: { case: "Done" } } : door
      ),
    };
    await this.putCalendar();
  }

  async markOpen(day: number) {
    this.calendar = {
      ...this.calendar,
      doors: this.calendar.doors.map((door) =>
        door.day === day ? { ...door, state: { case: "Open" } } : door
      ),
    };
    await this.putCalendar();
  }

  async getCalendar(): Promise<Calendar> {
    const config = await this.getAxiosConfig();
    const response = await axios.get<Calendar>("/api/calendars", config);
    return response.data;
  }

  async putCalendar(): Promise<Calendar> {
    const config = await this.getAxiosConfig();
    const response = await axios.put<Calendar>(
      "/api/calendars",
      this.calendar,
      config
    );
    return response.data;
  }

  async mounted(): Promise<any> {
    this.calendar = await this.getCalendar();
  }
}
</script>