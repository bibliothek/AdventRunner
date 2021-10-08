<template>
  <div
    class="
      m-2
      md:m-4
      w-24
      h-24
      md:w-40
      md:h-40
      rounded-box
      text-center text-gray-500
    "
    :class="getCardStyle()"
  >
    <div class="text-md md:text-4xl pt-1 md:pt-8">{{ day }}</div>
    <div class="text-xl md:text-2xl pt-1 font-bold text-primary md:pt-2">
      {{ distance }} km
    </div>
    <div class="md:hidden pt-1" v-if="isDone">🎉</div>
    <div class="md:hidden pt-1 flex flex-row" v-else>
      <div class="flex-grow"></div>
      <img class="w-6" src="../../public/runner.png" alt="Done" />
      <div class="flex-grow"></div>
    </div>
    <div class="hidden md:block">
      <div class="font-bold text-primary-focus pt-2" v-if="!isDone">
        Done
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          class="inline-block w-6 h-6 ml-2 stroke-current"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M9 5l7 7-7 7"
          ></path>
        </svg>
      </div>
      <div class="font pt-2" v-else>Done🎉</div>
    </div>
  </div>
</template>
<script lang="ts">
import { Options, Vue } from "vue-class-component";
@Options({
  props: {
    day: Number,
    distance: Number,
    isDone: Boolean,
  },
})
export default class ClosedDoor extends Vue {
  day!: number;
  distance!: number;
  isDone!: boolean;

  getCardStyle(): string {
    if (this.isDone!) {
      return "bg-gray-200 shadow-md";
    }
    return "bg-gray-100 shadow-xl";
  }
}
</script>