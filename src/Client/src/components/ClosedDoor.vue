<template>
  <button @click="$emit('opened')">
    <div
      class="
        m-4
        rounded-box
        text-center
        shadow-xl
        bg-primary
        text-primary-content
        w-40
        h-40
      "
    >
      <div class="text-4xl pt-8">{{ day }}</div>
      <div class="font-bold pt-10">
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