<template>
  <button @click="$emit('opened')">
    <div
      class="
        m-2
        md:m-4
        w-12
        h-12
        md:w-40
        md:h-40
        rounded-box
        text-center
        shadow-xl
        bg-neutral
        text-neutral-content
      "
    >
      <div class="text-2xl md:text-4xl pt-2 md:pt-8">{{ day }}</div>
      <div class="invisible md:visible font-bold pt-10">
        Open
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
    </div>
  </button>
</template>
<script lang="ts">
import { Options, Vue } from "vue-class-component";
@Options({
  props: {
    day: Number,
  },
  emits: ['opened']
})
export default class ClosedDoor extends Vue {
    day!: number;
}
</script>