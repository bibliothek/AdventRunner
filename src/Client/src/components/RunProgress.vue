<template>
    <div class="my-4 md:my-8">
        <div style="display: none" id="screenshot-title" class="mb-8 text-center">
            <h1 class="text-5xl text-primary font-bold">adventrunner.com</h1>
        </div>

        <div class="rounded-3xl border border-ink-100 bg-ink-50 p-4 md:p-6">
            <div class="flex flex-wrap items-end justify-between gap-x-4 gap-y-2 mb-3 md:mb-4">
                <div>
                    <div class="text-[11px] md:text-xs font-semibold uppercase tracking-widest text-ink-400">
                        Distance run
                    </div>
                    <div class="text-2xl md:text-4xl font-bold text-ink-700 tnum leading-tight">
                        {{ getKmByState("Done") }}
                        <span class="text-base md:text-xl font-medium text-ink-400">
                            of {{ getDistanceText(totalDistance) }}
                        </span>
                    </div>
                </div>
                <div class="text-3xl md:text-4xl font-bold text-primary tnum leading-none">
                    {{ donePercentage }}%
                </div>
            </div>

            <div class="h-7 md:h-8">
                <div
                    class="text-xs rounded-full overflow-hidden font-semibold text-center leading-7 md:leading-8 h-full flex flex-row bg-ink-100"
                >
                    <div :title="getKmByState('Done')" class="bg-primary text-primary-content myOverflow"
                         :style="doneWidth">
                        <span>{{ getKmByState("Done") }}</span>
                    </div>
                    <div :title="getKmByState('Open')" class="bg-warning text-neutral myOverflow"
                         :style="openWidth">
                        <span>{{ getKmByState("Open") }}</span>
                    </div>
                    <div :title="getKmByState('Closed')" class="bg-neutral text-neutral-content myOverflow"
                         :style="closedWidth">
                        <span>{{ getKmByState("Closed") }}</span>
                    </div>
                </div>
            </div>

            <div class="mt-3" v-if="hasVerifiedDistance">
                <div class="flex items-center gap-1.5 mb-1.5 text-[11px] md:text-xs font-semibold uppercase tracking-widest text-strava">
                    <span class="flex h-4 w-4 items-center justify-center rounded bg-strava">
                        <img class="w-2.5" src="/strava-icon.png" alt="" />
                    </span>
                    Verified on Strava
                </div>
                <div class="h-7 md:h-8">
                    <div
                        class="text-xs rounded-full overflow-hidden font-semibold text-center leading-7 md:leading-8 h-full flex flex-row">
                        <div :title="`${getDistanceText(verifiedDistance)} on Strava`"
                             class="bg-primary text-primary-content myOverflow flex flex-row items-center justify-center gap-1"
                             style="height:100%" :style="`width: ${verifiedPercentage}%`">
                            <img v-if="!hasVerifiedDistanceLessThan50Percent" style="height: 60%"
                                 src="/strava-icon.png" alt="">
                            <span>{{ getDistanceText(verifiedDistance) }}</span>
                        </div>
                        <div :title="getDistanceText(missingVerifiedDistance)"
                             class="text-neutral-content myOverflow flex flex-row items-center justify-center gap-1"
                             style="height:100%; background-color:#fc4c02" :style="`width: ${100 - verifiedPercentage}%`">
                            <img v-if="hasVerifiedDistanceLessThan50Percent" style="height: 60%"
                                 src="/strava-icon.png" alt="">
                            <span>{{ getDistanceText(missingVerifiedDistance) }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div v-if="!isSharedCalendarView" id="share-btn" class="mt-4 flex flex-col sm:flex-row gap-2 sm:justify-center">
                <button v-if="isCompleted" class="btn btn-secondary w-full sm:w-auto" @click="showCelebration">
                    🎉 Celebrate
                </button>
                <button class="btn btn-primary w-full sm:w-auto gap-2" @click="screenshot">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="w-5 h-5 stroke-current">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                              d="M4 16v2a2 2 0 002 2h12a2 2 0 002-2v-2M12 4v12m0-12l-4 4m4-4l4 4" />
                    </svg>
                    Share progress
                </button>
            </div>
        </div>

        <!-- Completion Popup -->
        <CompletionPopup
            :show="showCompletionPopup"
            :totalDistance="totalDistance"
            :hasVerifiedDistance="hasVerifiedDistance"
            :verifiedDistance="verifiedDistanceOption"
            :takingScreenshot="takingScreenshot"
            :year="year"
            @close="closeCompletionPopup"
            @share="shareCompletion"
        />
    </div>
</template>
<style lang="postcss">
.myOverflow {
    @apply text-ellipsis whitespace-nowrap overflow-hidden
}
</style>
<script lang="ts">
import { defineComponent } from "vue";
import { Calendar, DoorStateCase } from "../models/calendar"
import { getSome, isSome } from "../models/fsharp-helpers";
import html2canvas from "html2canvas";
import { sharedCalendarRoute } from "../router/router";
import CompletionPopup from "./CompletionPopup.vue";
import * as actionTypes from '../store/action-types';

let getTotal = (cal: Calendar) => {
    return cal.doors.reduce((val, el) => val + el.distance, 0)
}

let getByState = (cal: Calendar, state: DoorStateCase) => {
    return cal.doors.reduce((val, el) => el.state.Case === state ? val + el.distance : val, 0)
}

let getWidthPropertyForState = (cal: Calendar, state: DoorStateCase) => {
    const total = getTotal(cal as Calendar);
    const percent = total === 0 ? 0 : (getByState(cal as Calendar, state) / total) * 100;
    return `width: ${percent}%`;
}

export default defineComponent({
    name: "RunProgressComponent",
    components: {
        CompletionPopup
    },
    props: {
        cal: Object as () => Calendar,
        year: {type: Number, required: true}
    },
    data() {
        return {
            showCompletionPopup: false,
            takingScreenshot: false
        }
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
        donePercentage() {
            const total = getTotal(this.cal!);
            if (total === 0) {
                return 0;
            }
            return Math.round((getByState(this.cal!, "Done") / total) * 100);
        },
        hasVerifiedDistance() {
            return isSome(this.cal!.verifiedDistance);
        },
        hasVerifiedDistanceLessThan50Percent() {
            return isSome(this.cal!.verifiedDistance) && this.verifiedPercentage < 50;
        },
        verifiedPercentage() {
            return Math.min((this.verifiedDistance / (this.verifiedDistance + this.missingVerifiedDistance)) * 100, 100);
        },
        verifiedDistance() {
            const distance = getSome(this.cal!.verifiedDistance!);
            return (distance / 1000);
        },

        verifiedDistanceOption() {
            if (this.hasVerifiedDistance) {
                return this.verifiedDistance;
            }
            return undefined;
        },
        missingVerifiedDistance() {
            return Math.max(getTotal(this.cal!) * this.cal!.settings.distanceFactor - this.verifiedDistance, 0);
        },
        isSharedCalendarView() {
            return this.$route.name === sharedCalendarRoute;
        },
        totalDistance() {
            return getTotal(this.cal!) * this.cal!.settings.distanceFactor;
        },
        isCompleted() {
            return getByState(this.cal!, "Done") == getTotal(this.cal!)
                || this.hasVerifiedDistance && this.missingVerifiedDistance === 0;
        },
        hasShownCompletion() {
            return this.cal!.hasSeenCompletionPopup;
        }
    },
    watch: {
        isCompleted(newValue) {
            // Show popup once when completed, unless it's a shared calendar view
            if (newValue && !this.hasShownCompletion && !this.isSharedCalendarView) {
                this.showCompletionPopup = true;
            }
        }
    },
    mounted() {
        if (!this.hasShownCompletion && !this.isSharedCalendarView && this.isCompleted) {
            this.showCompletionPopup = true;
        }
    },
    methods: {
        getKmByState(state: DoorStateCase) {
            const distance = getByState(this.cal!, state) * this.cal!.settings.distanceFactor;
            return this.getDistanceText(distance);
        },
        getDistanceText(distance: number) {
            const n = Number.isInteger(distance) ? distance : distance.toFixed(1);
            const totalDistance = getTotal(this.cal!) * this.cal!.settings.distanceFactor;
            if (n > totalDistance) {
                return `${totalDistance}+ km`
            }
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
                ignoreElements: (el) => el.id === 'share-btn' || el.id === 'navbar' || el.id === 'door-hint',
                scale: 2.0,
                windowWidth: 500,
            }).then(canvas => {
                screenshotHeader.style.display = 'none';
                style.remove();
                const link = document.createElement('a');
                link.href = canvas.toDataURL('image/png');
                link.download = 'adventrunner-progress.png';
                link.click();
            });
        },
        async screenshotCompletion() {
            // Disable animations for screenshot
            this.takingScreenshot = true;

            // Wait a bit for DOM to update
            await new Promise(resolve => setTimeout(resolve, 100));

            const element = document.getElementById('completion-dialog') as HTMLElement;
            html2canvas(element, {
                scale: 4.0,
                windowWidth: 600,
            }).then(canvas => {
                // Crop the canvas to remove any white edge artifacts
                const croppedCanvas = document.createElement('canvas');
                const ctx = croppedCanvas.getContext('2d');

                // Crop pixels from all sides to remove white lines
                const cropTop = 4; // pixels to crop from top
                const cropBottom = 4; // pixels to crop from bottom
                const cropLeft = 2; // pixels to crop from left
                const cropRight = 2; // pixels to crop from right

                croppedCanvas.width = canvas.width - cropLeft - cropRight;
                croppedCanvas.height = canvas.height - cropTop - cropBottom;

                ctx?.drawImage(
                    canvas,
                    cropLeft, cropTop, // source x, y
                    croppedCanvas.width, croppedCanvas.height, // source width, height
                    0, 0, // destination x, y
                    croppedCanvas.width, croppedCanvas.height // destination width, height
                );

                const link = document.createElement('a');
                link.href = croppedCanvas.toDataURL('image/png');
                link.download = 'adventrunner-completion.png';
                link.click();

                // Restore dialog after screenshot
                this.takingScreenshot = false;
            });
        },
        async closeCompletionPopup() {
            this.showCompletionPopup = false;
            if(!this.cal?.hasSeenCompletionPopup) {
                await this.$store.dispatch(actionTypes.SET_COMPLETION_SHOWN);
            }
        },
        showCelebration() {
            this.showCompletionPopup = true;
        },
        async shareCompletion() {
            await this.screenshotCompletion();
            await this.closeCompletionPopup();
        }
    }
})
</script>