<template>
    <div class="m-8">
        <div style="display: none" id="screenshot-title" class="h-8 mb-12 flex flex-row">
            <div class="flex-grow"></div>
            <h1 class="text-5xl text-primary font-bold">adventrunner.com</h1>
            <div class="flex-grow"></div>
        </div>
        <div class="h-8">
            <div
                class="text-xs rounded-lg overflow-hidden font-semibold text-center leading-8 mx-auto h-full max-w-2xl flex flex-row"
            >
                <div :title="getKmByState('Done')" class="bg-primary text-primary-content myOverflow"
                     :style="doneWidth">
                    <span>{{ getKmByState("Done") }}</span>
                </div>
                <div :title="getKmByState('Open')" class=" bg-warning text-waring myOverflow"
                     :style="openWidth">
                    <span>{{ getKmByState("Open") }}</span>
                </div>
                <div :title="getKmByState('Closed')" class=" bg-neutral text-neutral-content myOverflow"
                     :style="closedWidth">
                    <span>{{ getKmByState("Closed") }}</span>
                </div>
            </div>
        </div>
        <div class="h-8 mt-2" v-if="hasVerifiedDistance">
            <div
                class="text-xs rounded-lg overflow-hidden font-semibold text-center leading-8 mx-auto h-full max-w-2xl flex flex-row">
                <div :title="`${getDistanceText(verifiedDistance)} on Strava`"
                     class="bg-primary text-primary-content myOverflow flex flex-row"
                     style="height:100%" :style="`width: ${verifiedPercentage}%`">
                    <div class="flex-grow"></div>
                    <img class="my-auto" style="height: 80%" src="../../public/strava-icon.png">
                    <span>{{ getDistanceText(verifiedDistance) }}</span>
                    <div class="flex-grow"></div>
                </div>
                <div :title="getDistanceText(missingVerifiedDistance)"
                     class="text-neutral-content myOverflow flex flex-row"
                     style="height:100%; background-color:#fc4c02" :style="`width: ${100 - verifiedPercentage}%`">
                    <div class="flex-grow"></div>
                    <img v-if="hasVerifiedDistanceZero" class="my-auto" style="height: 80%"
                         src="/strava-icon.png">
                    <span>{{ getDistanceText(missingVerifiedDistance) }}</span>
                    <div class="flex-grow"></div>
                </div>
            </div>
        </div>
        <div v-if="!isSharedCalendarView" id="share-btn" class="h-8 mt-2 flex flex-row">
            <div class="flex-grow"></div>
            <button class="btn btn-primary" @click="screenshot">
                Share progress
            </button>
            <div class="flex-grow"></div>
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
import html2canvas from "html2canvas";
import {sharedCalendarRoute} from "../router/router";

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
        hasVerifiedDistanceZero() {
            return isSome(this.cal!.verifiedDistance) && getSome(this.cal!.verifiedDistance!) === 0.0;
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
        },
        isSharedCalendarView() {
            return this.$route.name === sharedCalendarRoute;
        },
    },
    methods: {
        getKmByState(state: DoorStateCase) {
            const distance = getByState(this.cal!, state) * this.cal!.settings.distanceFactor;
            return this.getDistanceText(distance);
        },
        getDistanceText(distance: number) {
            const n = Number.isInteger(distance) ? distance : distance.toFixed(1);
            return `${n} km`
        },
        screenshot() {
            const element = document.getElementsByClassName('container')[0] as HTMLElement;
            const screenshotHeader = document.getElementById('screenshot-title')!;

            const style = document.createElement('style');
            document.head.appendChild(style);
            style.sheet?.insertRule('body > div:last-child img { display: inline-block; }');

            screenshotHeader.style.display = '';
            html2canvas(element, {
                ignoreElements: (el) => el.id === 'share-btn' || el.id === 'navbar',
                scale: 2.0,
                windowWidth: 700,
            }).then(canvas => {
                screenshotHeader.style.display = 'none';
                style.remove();
                const link = document.createElement('a');
                link.href = canvas.toDataURL('image/png');
                link.download = 'adventrunner-progress.png';
                link.click();
            });
        }
    }
})
</script>