<template>
  <div>
  <div class="card max-w-sm border-2 border-base-300">
    <div class="card-body">
      <div class="form-control">
        <label class="label">
          <span class="label-text">Distance</span>
        </label>
        <select class="select select-bordered w-full max-w-xs" v-model="selectedFactor">
          <option>Chose you preferred dinstance</option>
          <option value="1">Normal</option>
          <option value="0.5">Half it</option>
          <option value="2">Double it</option>
        </select>
      </div>
    </div>
  </div>
  <button @click="resetCalendar" class="btn btn-error btn-outline border-error text-error mt-5">Reset Calendar</button>
    <div class="alert alert-warning mt-2">
      <div class="flex-1">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="w-6 h-6 mx-2 stroke-current">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
        </svg>
        <label class="">Resets the entire calendar and it's data!</label>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import * as actionTypes from '../store/action-types';
import {
    defineComponent,
} from "@vue/runtime-core";

export default defineComponent({
    name: "SettingsComponent",
    computed: {
        selectedFactor: {
            get() {
                return this.$store.state.userData.calendars[this.$store.state.displayPeriod]!.settings.distanceFactor;
            },
            set(value: string) {
                const number = Number(value);
                if(isNaN(number)){
                    return;
                }
                this.$store.dispatch(actionTypes.SET_SCALE_FACTOR, value)
            }
        }
    },
    methods: {
        resetCalendar() {
            this.$store.dispatch(actionTypes.RESET_CALENDAR)
        }
    },
    mounted() {
        this.$store.dispatch(actionTypes.GET_CALENDAR)
    }
})
</script>