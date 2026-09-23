<template>
    <div>
        <div class="card max-w-xl border-2 border-base-300">
            <div class="card-body">

                <div class="form-control">
                    <label class="label">
                        <span class="label-text">Display name</span>
                    </label>
                    <div class="flex items-center">
                        <input type="text" @keydown="displayNameKeyDown" placeholder="Your name"
                            class="input w-4/5 max-w-xs input-bordered" v-model="displayName" />
                        <font-awesome-icon icon="fa-solid fa-spinner" class="animate-spin m-4"
                            v-if="displayNameIcon === DisplayNameIcon.Processing" />
                        <font-awesome-icon icon="far fa-check-square" class="m-4 text-success"
                            v-if="displayNameIcon === DisplayNameIcon.Done" />
                    </div>
                </div>
                <div class="form-control mt-8">
                    <label class="label">
                        <span class="label-text">Distance</span>
                    </label>
                    <select class="select select-bordered w-4/5 max-w-xs" v-model="selectedFactor">
                        <option value="1">Normal</option>
                        <option value="0.5">Half it</option>
                        <option value="2">Double it</option>
                    </select>
                </div>
                <div class="form-control mt-8">
                    <label class="label">
                        <span class="label-text">Display Mode</span>
                    </label>
                    <select class="select select-bordered w-4/5 max-w-xs" v-model="displayType">
                        <option value="1">Doors</option>
                        <option value="0">Calendar</option>
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