<template>
<div class="h-4 rounded-box flex flex-row">

    <div class="bg-primary rounded-l-lg" style="height:100%" :style="doneWidth"></div>
    <div class="bg-warning" style="33%; height:100%" :style="openWidth"></div>
    <div class="bg-neutral rounded-r-lg" style="33%; height:100%" :style="closedWidth"></div>
</div>
</template>
<script lang="ts">
import { defineComponent } from "vue";
import {Calendar, emptyCalendar, Door, DoorState, DoorStateCase} from "../models/calendar"

let getTotal = (cal: Calendar) => {
    return cal.doors.reduce((val, el) =>  val + el.distance, 0)
}

let getByState = (cal: Calendar, state: DoorStateCase) => {
    return cal.doors.reduce((val, el) => el.state.case === state ? val + el.distance : val, 0)
}

export default defineComponent({
    name: "RunProgressComponent",
    props: {
        cal: {}
    },
    computed: {
        doneWidth() {
            const percentageDone = (getByState(this.cal as Calendar, "Done") / getTotal(this.cal as Calendar)) * 100;
            console.log(percentageDone)
            return `width: ${percentageDone}%`;
        },
        openWidth() {
            const percentageDone = (getByState(this.cal as Calendar, "Open") / getTotal(this.cal as Calendar)) * 100;
            return `width: ${percentageDone}%`;
        },
        closedWidth() {
            const percentageDone = (getByState(this.cal as Calendar, "Closed") / getTotal(this.cal as Calendar)) * 100;
            return `width: ${percentageDone}%`;
        },
    }
})
</script>