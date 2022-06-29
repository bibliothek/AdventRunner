<template>
    <div class="h-8 m-8">
        <div class="mx-auto h-full max-w-2xl rounded-box flex flex-row">

            <div class="text-xs bg-primary rounded-l-lg text-center text-primary-content" style="height:100%"
                :style="doneWidth"><span class="">{{ getKmByState("Done") }}</span></div>
            <div class="text-xs bg-warning text-center text-waring" style="33%; height:100%" :style="openWidth">
                <span class="">{{ getKmByState("Open") }}</span></div>
            <div class="text-xs bg-neutral text-center text-neutral-content rounded-r-lg" style="33%; height:100%"
                :style="closedWidth"><span class="">{{ getKmByState("Closed") }}</span></div>
        </div>
    </div>
</template>
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