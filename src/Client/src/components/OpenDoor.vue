<template>
    <div
      class="
        m-4
        rounded-box
        text-center
        text-gray-500
        w-40
        h-40
      "

      :class="getCardStyle()"
    >
      <div class="text-4xl pt-8">{{ day }}</div>
      <div class="text-2xl font-bold text-primary pt-2">{{ distance }} km</div>
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
</template>
<script lang="ts">
import { Options, Vue } from "vue-class-component";
@Options({
  props: {
    day: Number,
    distance: Number,
    isDone: Boolean
  },
})
export default class ClosedDoor extends Vue {
    day!: number;
    distance!: number;
    isDone!: boolean;

    getCardStyle(): string {
      if(this.isDone!) {
        return "bg-gray-200 shadow-md";
      }
      return "bg-gray-100 shadow-xl";
    }
}
</script>