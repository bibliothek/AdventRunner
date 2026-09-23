<template>
    <div class="my-4 md:my-8 max-w-xl">
        <h1 class="text-2xl sm:text-3xl font-bold tracking-tight text-ink-800 mb-4 sm:mb-6">Settings</h1>
        <div class="rounded-3xl border border-ink-100 bg-ink-50 p-4 sm:p-6 flex flex-col gap-6">

            <div class="form-control">
                <label class="label pt-0" for="settings-display-name">
                    <span class="text-xs font-semibold uppercase tracking-widest text-ink-400">Display name</span>
                </label>
                <div class="flex items-center gap-3">
                    <input id="settings-display-name" type="text" @keydown="displayNameKeyDown" placeholder="Your name"
                        class="input input-bordered border-ink-100 rounded-xl bg-white w-full max-w-xs" v-model="displayName" />
                    <font-awesome-icon icon="fa-solid fa-spinner" class="animate-spin text-ink-400"
                        v-if="displayNameIcon === DisplayNameIcon.Processing" />
                    <font-awesome-icon icon="far fa-check-square" class="text-primary"
                        v-if="displayNameIcon === DisplayNameIcon.Done" />
                </div>
            </div>
            <div class="form-control">
                <label class="label pt-0" for="settings-distance">
                    <span class="text-xs font-semibold uppercase tracking-widest text-ink-400">Distance</span>
                </label>
                <select id="settings-distance" class="select select-bordered border-ink-100 rounded-xl bg-white w-full max-w-xs"
                    v-model="selectedFactor">
                    <option value="1">Normal</option>
                    <option value="0.5">Half it</option>
                    <option value="2">Double it</option>
                </select>
            </div>
            <div class="form-control">
                <label class="label pt-0" for="settings-display-mode">
                    <span class="text-xs font-semibold uppercase tracking-widest text-ink-400">Display mode</span>
                </label>
                <select id="settings-display-mode" class="select select-bordered border-ink-100 rounded-xl bg-white w-full max-w-xs"
                    v-model="displayType">
                    <option value="1">Doors</option>
                    <option value="0">Calendar</option>
                </select>
            </div>
            <div class="form-control">
                <label class="label pt-0 cursor-pointer justify-start gap-4">
                    <span class="text-xs font-semibold uppercase tracking-widest text-ink-400">Shareable link</span>
                    <input type="checkbox" class="toggle toggle-primary" v-model="hasShareableLink" />
                </label>
                <div class="flex items-center gap-1 rounded-xl border border-ink-100 bg-white pl-3 pr-1 py-1"
                    v-if="hasShareableLink">
                    <a class="link link-hover text-ink-600 block truncate min-w-0 flex-1" :href="sharedLinkValue">{{
                        sharedLinkValue }}</a>
                    <div class="tooltip" :data-tip="copyToClipboardTooltip" v-on:mouseenter="resetClipboardTooltip">
                        <button class="btn btn-ghost btn-sm" aria-label="Copy link" @click="copyLinkToClipboard">
                            <font-awesome-icon icon="fas fa-copy" />
                        </button>
                    </div>
                </div>
                <p class="text-sm text-ink-400" v-else>Turn on to get a link others can use to view your calendar.</p>
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
import { DisplayType } from '../models/calendar';

const inputDebounceInMs = 300;

enum DisplayNameIcon {
    None,
    Processing,
    Done,
}

export default defineComponent({
    name: "SettingsComponent",
    data() {
        return {
            copyToClipboardTooltip: "Copy to clipboard",
            typingTimerValue: setTimeout(() => { }, 0),
            typingTimerIcon: setTimeout(() => { }, 0),
            displayNameIcon: DisplayNameIcon.None,
            DisplayNameIcon
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
        displayType: {
            get() {
                return this.$store.getters.displayType;
            },
            set(value: DisplayType) {
                this.$store.dispatch(actionTypes.SET_DISPLAY_TYPE, value);
            }
        },
        displayName: {
            get() {
                return this.$store.getters.displayName;
            },
            set(val: string) {
                clearTimeout(this.typingTimerValue);
                this.typingTimerValue = setTimeout(() => {
                    this.$store.dispatch(actionTypes.SET_DISPLAY_NAME, val)
                }, inputDebounceInMs);

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
        async copyLinkToClipboard() {
            await navigator.clipboard.writeText(this.sharedLinkValue);
            this.copyToClipboardTooltip = "Copied!";
        },
        resetClipboardTooltip() {
            this.copyToClipboardTooltip = "Copy to clipboard";
        },
        displayNameKeyDown() {
            clearTimeout(this.typingTimerIcon);
            this.displayNameIcon = DisplayNameIcon.Processing;
            this.typingTimerIcon = setTimeout(() => {
                this.displayNameIcon = DisplayNameIcon.Done;
                this.typingTimerIcon = setTimeout(() => {
                    this.displayNameIcon = DisplayNameIcon.None;
                }, 1000);
            }, inputDebounceInMs);
        }

    },
    mounted() {
        this.$store.dispatch(actionTypes.GET_CALENDAR)
    }
})

</script>