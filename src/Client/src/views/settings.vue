<template>
    <div>
        <div class="card max-w-xl border-2 border-base-300">
            <div class="card-body">

                <div class="form-control">
                    <label class="label">
                        <span class="label-text">Display name</span>
                    </label>
                    <div class="flex items-center">
                        <input type="text" placeholder="Your name" class="input w-4/5 max-w-xs input-bordered"
                            v-model="displayName" />
                        <font-awesome-icon
                            icon="fa-solid fa-spinner" class="animate-spin m-4" :class="displayNameProgressIconDisplay" />
                        <font-awesome-icon
                            icon="far fa-check-square" class="m-4 text-success" :class="displayNameDoneIconDisplay" />
                    </div>
                </div>
                <div class="form-control mt-8">
                    <label class="label">
                        <span class="label-text">Distance</span>
                    </label>
                    <select class="select select-bordered w-4/5 max-w-xs" v-model="selectedFactor">
                        <option>Chose you preferred dinstance</option>
                        <option value="1">Normal</option>
                        <option value="0.5">Half it</option>
                        <option value="2">Double it</option>
                    </select>
                </div>
                <div class="form-control mt-8">
                    <label class="label">
                        <span class="label-text">Shareable link</span>
                    </label>
                    <input type="checkbox" class="toggle" v-model="hasShareableLink" />
                    <div class="flex items-center border-2 bg-gray-50 rounded my-2 px-2" v-if="hasShareableLink">
                        <div class="w-4/5 flex-grow">
                            <a class="link link-hover block truncate" :href="sharedLinkValue">{{ sharedLinkValue }}</a>
                        </div>
                        <div class="tooltip" :data-tip="copyToClipboardTooltip" v-on:mouseenter="resetClipboardTooltip">
                            <button class="btn btn-ghost" @click="copyLinkToClipboard">
                                <font-awesome-icon icon="fas fa-copy" />
                            </button>
                        </div>
                    </div>
                    <div class="" v-else>Create link for sharing</div>
                </div>
            </div>
        </div>
        <button @click="resetCalendar" class="btn btn-error btn-outline border-error text-error mt-5">Reset
            Calendar</button>
        <div class="alert alert-warning mt-2">
            <div class="flex-1">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                    class="w-6 h-6 mx-2 stroke-current">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z">
                    </path>
                </svg>
                <label class="">Resets the entire calendar and it's data!</label>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import * as actionTypes from '../store/action-types';
import {
    defineComponent,
} from "@vue/runtime-core";
import { isSome } from "../models/fsharp-helpers";
import { mapGetters } from 'vuex';

export default defineComponent({
    name: "SettingsComponent",
    data() {
        return {
            copyToClipboardTooltip: "Copy to clipboard",
            typingTimer: setTimeout(() => { }, 0),
            displayNameProgressIconDisplay: "hidden",
            displayNameDoneIconDisplay: "hidden",
        }
    },
    computed: {
        ...mapGetters(['sharedLinkValue']),
        selectedFactor: {
            get() {
                return this.$store.getters.displayCalendar.settings.distanceFactor;
            },
            set(value: string) {
                const number = Number(value);
                if (isNaN(number)) {
                    return;
                }
                this.$store.dispatch(actionTypes.SET_SCALE_FACTOR, value)
            }
        },
        displayName: {
            get() {
                return this.$store.getters.displayName;
            },
            set(val: string) {
                clearTimeout(this.typingTimer);
                this.displayNameProgressIconDisplay = "";
                this.displayNameDoneIconDisplay = "hidden";
                this.typingTimer = setTimeout(() => {
                    this.$store.dispatch(actionTypes.SET_DISPLAY_NAME, val)
                    this.displayNameDoneIconDisplay = "";
                    this.displayNameProgressIconDisplay = "hidden";

                }, 500);

            }
        },
        hasShareableLink: {
            get() {
                return !!this.$store.getters.displayCalendar.settings.sharedLinkId && isSome(this.$store.getters.displayCalendar.settings.sharedLinkId);
            },
            set(value: boolean) {
                if (value) {
                    this.$store.dispatch(actionTypes.ENABLE_SHARED_LINK);
                } else {
                    this.$store.dispatch(actionTypes.DISABLE_SHARED_LINK);
                }
            }
        },

    },
    methods: {
        resetCalendar() {
            this.$store.dispatch(actionTypes.RESET_CALENDAR)
        },
        async copyLinkToClipboard() {
            await navigator.clipboard.writeText(this.sharedLinkValue);
            this.copyToClipboardTooltip = "Copied!";
        },
        resetClipboardTooltip() {
            this.copyToClipboardTooltip = "Copy to clipboard";
        },

    },
    mounted() {
        this.$store.dispatch(actionTypes.GET_CALENDAR)
    }
})

</script>