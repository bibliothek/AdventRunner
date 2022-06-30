<template>
    <div class="h-8 m-8">
        <div class="text-xs rounded-lg overflow-hidden font-semibold text-center leading-8 mx-auto h-full max-w-2xl flex flex-row">
            <div class="bg-primary text-primary-content myOverflow" style="height:100%" :style="doneWidth">
                <span :title="getKmByState('Done')">{{ getKmByState("Done") }}</span>
            </div>
            <div class=" bg-warning text-waring myOverflow" style="33%; height:100%" :style="openWidth">
                <span :title="getKmByState('Open')">{{ getKmByState("Open") }}</span>
            </div>
            <div class=" bg-neutral text-neutral-content myOverflow" style="33%; height:100%"
                :style="closedWidth">
                <span :title="getKmByState('Closed')">{{ getKmByState("Closed") }}</span>
            </div>
        </div>
    </div>
</template>
<style lang="postcss">
.myOverflow {
    @apply text-ellipsis whitespace-nowrap overflow-hidden
}
</style>
<script lang="ts">
import { defineComponent } from "vue";
import { Calendar, emptyCalendar, Door, DoorState, DoorStateCase } from "../models/calendar"

let getTotal = (cal: Calendar) => {
    return cal.doors.reduce((val, el) => val + el.distance, 0)
}

let getByState = (cal: Calendar, state: DoorStateCase) => {
    return cal.doors.reduce((val, el) => el.state.case === state ? val + el.distance : val, 0)
}

let getWidthPropertyForState = (cal: Calendar, state: DoorStateCase) => {
    const percent = (getByState(cal as Calendar, state) / getTotal(cal as Calendar)) * 100;
    return `width: ${percent}%`;
}

export default defineComponent({
    name: "RunProgressComponent",
    props: {
        cal: Object as () => Calendar
    },
    computed: {
        doneWidth() {
            return getWidthPropertyForState(this.cal!, "Done");
        },
        openWidth() {
            return getWidthPropertyForState(this.cal!, "Open");
        },
        closedWidth() {
            return getWidthPropertyForState(this.cal!, "Closed");
        },
    },
    methods: {
        getKmByState(state: DoorStateCase) {
            const distance = getByState(this.cal!, state) * this.cal!.settings.distanceFactor;
            return `${distance} km`;
        }
    }
})
</script>