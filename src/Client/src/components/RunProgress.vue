<template>
    <div class="m-8">
    <div class="h-8">
        <div class="text-xs rounded-lg overflow-hidden font-semibold text-center leading-8 mx-auto h-full max-w-2xl flex flex-row">
            <div :title="getKmByState('Done')" class="bg-primary text-primary-content myOverflow" style="height:100%" :style="doneWidth">
                <span >{{ getKmByState("Done") }}</span>
            </div>
            <div :title="getKmByState('Open')" class=" bg-warning text-waring myOverflow" style="33%; height:100%" :style="openWidth">
                <span >{{ getKmByState("Open") }}</span>
            </div>
            <div :title="getKmByState('Closed')" class=" bg-neutral text-neutral-content myOverflow" style="33%; height:100%"
                :style="closedWidth">
                <span >{{ getKmByState("Closed") }}</span>
            </div>
        </div>
    </div>
    <div class="h-8 mt-2" v-if="hasVerifiedDistance">
        <div class="text-xs rounded-lg overflow-hidden font-semibold text-center leading-8 mx-auto h-full max-w-2xl flex flex-row">
            <div :title="getDistanceText(verifiedDistance)" class="bg-primary text-primary-content myOverflow" style="height:100%" :style="`width: ${verifiedPercentage}%`">
                <span >{{ getDistanceText(verifiedDistance) }}</span>
            </div>
            <div :title="getDistanceText(missingVerifiedDistance)" class=" bg-neutral text-neutral-content myOverflow" style="33%; height:100%"
                :style="`width: ${100 - verifiedPercentage}%`">
                <span >{{ getDistanceText(missingVerifiedDistance) }}</span>
            </div>
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
import {defineComponent} from "vue";
import {Calendar, DoorStateCase} from "../models/calendar"
import {getSome, isSome} from "../models/fsharp-helpers";

let getTotal = (cal: Calendar) => {
    return cal.doors.reduce((val, el) => val + el.distance, 0)
}

let getByState = (cal: Calendar, state: DoorStateCase) => {
    return cal.doors.reduce((val, el) => el.state.Case === state ? val + el.distance : val, 0)
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
        hasVerifiedDistance() {
            return isSome(this.cal!.verifiedDistance);
        },
        verifiedPercentage() {
            return Math.min((this.verifiedDistance / (this.verifiedDistance + this.missingVerifiedDistance)) * 100, 100);
        },
        verifiedDistance() {
            const distance = getSome(this.cal!.verifiedDistance!);
            return (distance / 1000);
        },
        missingVerifiedDistance() {
            return Math.max(getTotal(this.cal!) * this.cal!.settings.distanceFactor - this.verifiedDistance, 0);
        }
    },
    methods: {
        getKmByState(state: DoorStateCase) {
            const distance = getByState(this.cal!, state) * this.cal!.settings.distanceFactor;
            return this.getDistanceText(distance);
        },
        getDistanceText(distance: number) {
            const n = Number.isInteger(distance) ? distance : distance.toFixed(1);
            return `${n} km`
        }
    }
})
</script>