<template>
    <Teleport to="body">
        <Transition name="fade">
            <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center">
                <!-- Backdrop -->
                <div class="absolute inset-0 bg-black bg-opacity-60 backdrop-blur-sm" @click="$emit('close')"></div>

                <!-- Dialog -->
                <div class="relative z-10 max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto">
                    <Transition name="pop">
                        <div id="completion-dialog" v-if="show" class="bg-gradient-to-br from-primary to-secondary rounded-3xl shadow-2xl overflow-hidden transform" :class="{ 'no-animations': takingScreenshot }">
                            <!-- Close button -->
                            <button
                                v-if="!takingScreenshot"
                                @click="$emit('close')"
                                class="absolute top-4 right-4 text-primary-content hover:text-white transition-colors z-10 text-2xl font-bold w-10 h-10 flex items-center justify-center rounded-full hover:bg-white hover:bg-opacity-20"
                            >
                                ×
                            </button>

                            <!-- Content -->
                            <div class="p-6 md:p-8 text-center text-primary-content">
                                <!-- Trophy Animation -->
                                <div class="mb-4 animate-bounce-slow trophy-container">
                                    <div class="text-6xl md:text-7xl inline-block animate-wiggle">🏆</div>
                                </div>

                                <!-- Main Message -->
                                <h1 class="text-3xl md:text-4xl font-bold mb-3 animate-slide-up">
                                    Congratulations!
                                </h1>

                                <div class="text-lg md:text-xl font-semibold mb-6 animate-slide-up animation-delay-100">
                                    You've completed your Adventrunner challenge!
                                </div>

                                <!-- Stats -->
                                <div class="bg-white bg-opacity-20 backdrop-blur-md rounded-2xl p-4 md:p-6 mb-6 animate-slide-up animation-delay-200">
                                    <div class="text-3xl md:text-4xl font-bold mb-2">
                                        {{ formatDistance(totalDistance) }}
                                    </div>
                                    <div class="text-base md:text-lg opacity-90">
                                        Total Distance Completed
                                    </div>

                                    <div v-if="hasVerifiedDistance" class="mt-4 pt-4 border-t border-white border-opacity-30">
                                        <div class="flex items-center justify-center gap-2">
                                            <img src="/strava-icon.png" alt="Strava" class="h-6 w-6">
                                            <span class="text-base font-semibold">Verified on Strava!</span>
                                        </div>
                                    </div>
                                </div>

                                <!-- Celebration Message -->
                                <div class="text-base md:text-lg mb-6 animate-slide-up animation-delay-300">
                                    <div class="mb-2">🚀 Look at you go! 🚀</div>
                                    <div class="text-sm md:text-base opacity-90">
                                        You just turned an advent calendar into your personal victory lap.
                                    </div>
                                </div>

                                <!-- Action Buttons -->
                                <div id="completion-action-btns" class="flex gap-3 justify-center animate-slide-up animation-delay-400">
                                    <button
                                        @click="$emit('share')"
                                        class="btn btn-md bg-white text-primary hover:bg-opacity-90 border-none shadow-lg text-base px-8"
                                    >
                                        Share
                                    </button>
                                    <button
                                        @click="$emit('close')"
                                        class="btn btn-md bg-white bg-opacity-80 text-primary hover:bg-opacity-90 border-none shadow-lg text-base px-8"
                                    >
                                        Close
                                    </button>
                                </div>

                                <!-- Website Reference -->
                                <div class="mt-6 text-primary-content text-opacity-70 text-sm animate-slide-up animation-delay-500">
                                    adventrunner.com
                                </div>
                            </div>
                        </div>
                    </Transition>
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
    name: 'CompletionPopup',
    props: {
        show: {
            type: Boolean,
            required: true
        },
        totalDistance: {
            type: Number,
            required: true
        },
        hasVerifiedDistance: {
            type: Boolean,
            default: false
        },
        verifiedDistance: {
            type: Number,
            default: 0
        },
        takingScreenshot: {
            type: Boolean,
            default: false
        }
    },
    emits: ['close', 'share'],
    setup() {
        const formatDistance = (distance: number) => {
            const n = Number.isInteger(distance) ? distance : distance.toFixed(1);
            return `${n} km`;
        };

        return {
            formatDistance
        };
    }
});
</script>

<style scoped>
/* Disable rounded corners and shadow when taking screenshot */
.no-animations {
    border-radius: 0 !important;
    box-shadow: none !important;
    outline: 1px solid transparent !important;
}

.no-animations * {
    animation: none !important;
    transition: none !important;
    opacity: 1 !important;
}

.no-animations *:not(.trophy-container) {
    transform: none !important;
}

/* Adjust trophy position when taking screenshot */
.no-animations .trophy-container {
    transform: translateY(-20px) !important;
}

/* Fade transition for backdrop */
.fade-enter-active, .fade-leave-active {
    transition: opacity 0.3s ease;
}

.fade-enter-from, .fade-leave-to {
    opacity: 0;
}

/* Pop transition for dialog */
.pop-enter-active {
    animation: pop-in 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
}

.pop-leave-active {
    animation: pop-out 0.3s ease-in;
}

@keyframes pop-in {
    0% {
        transform: scale(0.5) rotate(-5deg);
        opacity: 0;
    }
    70% {
        transform: scale(1.05) rotate(2deg);
    }
    100% {
        transform: scale(1) rotate(0);
        opacity: 1;
    }
}

@keyframes pop-out {
    0% {
        transform: scale(1);
        opacity: 1;
    }
    100% {
        transform: scale(0.8);
        opacity: 0;
    }
}

/* Bounce animation for trophy */
.animate-bounce-slow {
    animation: bounce-slow 2s ease-in-out infinite;
}

@keyframes bounce-slow {
    0%, 100% {
        transform: translateY(0);
    }
    50% {
        transform: translateY(-20px);
    }
}

/* Wiggle animation */
.animate-wiggle {
    animation: wiggle 1s ease-in-out infinite;
}

@keyframes wiggle {
    0%, 100% {
        transform: rotate(-5deg);
    }
    50% {
        transform: rotate(5deg);
    }
}

/* Slide up animations */
.animate-slide-up {
    animation: slide-up 0.6s ease-out forwards;
    opacity: 0;
}

@keyframes slide-up {
    from {
        transform: translateY(30px);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}

/* Animation delays */
.animation-delay-100 {
    animation-delay: 0.1s;
}

.animation-delay-200 {
    animation-delay: 0.2s;
}

.animation-delay-300 {
    animation-delay: 0.3s;
}

.animation-delay-400 {
    animation-delay: 0.4s;
}
</style>
