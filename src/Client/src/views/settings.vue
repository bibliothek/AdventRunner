<template>
  <div>
  <div class="card max-w-sm border-2 border-base-300">
    <div class="card-body">
      <div class="form-control">
        <label class="label">
          <span class="label-text">Distance</span>
        </label>
        <select class="select select-bordered w-full max-w-xs" v-model="selectedFactor">
          <option disabled="disabled" selected="selected">Chose you preferred dinstance</option>
          <option value="1">Normal</option>
          <option value="0.5">Half it</option>
          <option value="2">Double it</option>
        </select>
      </div>
      <div class="form-control mt-6">
        <input @click="save" type="button" value="Save" class="btn btn-primary">
      </div>
    </div>
  </div>
  <button @click="resetCalendar" class="btn btn-error mt-5">Reset Calendar</button>
    <div class="alert alert-warning">
      <div class="flex-1">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="w-6 h-6 mx-2 stroke-current">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
        </svg>
        <label>Resets the entire calendar and it's data!</label>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import {Vue} from "vue-class-component";
import { Calendar, emptyCalendar } from "../models/calendar";
import axios from "axios";
import { inject } from "@vue/runtime-core";

export default class SettingsComponent extends Vue {
  calendar = emptyCalendar();
  selectedFactor = '1';

  private Auth = inject("Auth") as any;

  private async getAxiosConfig(): Promise<any> {
    const token = await this.Auth.getTokenSilently();
    return {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    };
  }

  async getCalendar(): Promise<Calendar> {
    const config = await this.getAxiosConfig();
    const response = await axios.get<Calendar>("/api/calendars", config);
    return response.data;
  }

  async mounted(): Promise<any> {
    this.calendar = (await this.getCalendar());
    this.selectedFactor = this.calendar.settings.distanceFactor + "";
  }

  async save() {
    const factor = parseFloat(this.selectedFactor);
    this.calendar = {
      ...this.calendar,
      settings: {
        distanceFactor: factor
      },
    };
    await this.putCalendar();
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

  async resetCalendar(): Promise<Calendar> {
    const config = await this.getAxiosConfig();
    const response = await axios.post<Calendar>(
        "/api/calendars",
        this.calendar,
        config
    );
    return response.data;
  }
}
</script>

<style scoped>

</style>