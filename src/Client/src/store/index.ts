import { Auth0Client } from "@auth0/auth0-spa-js";
import axios from "axios";
import { inject } from "vue";
import { ActionContext, createLogger, createStore } from "vuex";
import {
    Calendar,
    emptyUserData,
    SharedLinkPostRequest,
    UserData,
} from "../models/calendar";
import * as actionTypes from "./action-types";
import * as mutationTypes from "./mutation-types.";


export interface State {
    userData: UserData;
    displayPeriod: number;
    axiosConfig: any;
}

const state: State = {
    userData: emptyUserData(),
    displayPeriod: 0,
    axiosConfig: null,
};

const mutations = {
    [mutationTypes.OPEN_DOOR](state: State, day: number) {
        state.userData.calendars[state.displayPeriod]!.doors[
            day - 1
        ].state.case = "Open";
    },
    [mutationTypes.MARK_DOOR_DONE](state: State, day: number) {
        state.userData.calendars[state.displayPeriod]!.doors[
            day - 1
        ].state.case = "Done";
    },
    [mutationTypes.SET_CALENDAR](state: State, calendar: Calendar) {
        state.userData.calendars[state.displayPeriod] = calendar;
    },
    [mutationTypes.SET_SCALE_FACTOR](state: State, scaleFactor: number) {
        state.userData.calendars[state.displayPeriod]!.settings.distanceFactor =
            Number(scaleFactor);
    },
    [mutationTypes.SET_AUTH_HEADERS](state: State, config: any) {
        state.axiosConfig = config;
    },
    [mutationTypes.SET_DISPLAY_PERIOD](state: State, period: number) {
        state.displayPeriod = period;
    },
    [mutationTypes.SET_USER_DATA](state: State, userData: UserData) {
        state.userData = userData;
    },
    [mutationTypes.REMOVE_SHARED_LINK](state: State, period: number) {
        state.userData.calendars[period]!.settings.sharedLinkId = undefined;
    }
};

async function getAxiosConfig(): Promise<any> {
    const auth = inject("Auth") as Auth0Client;
    const token = await auth.getTokenSilently();
    return {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    };
}

const actions = {
    async [actionTypes.OPEN_DOOR](
        context: ActionContext<State, State>,
        day: number
    ) {
        context.commit(mutationTypes.OPEN_DOOR, day);
        await context.dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.MARK_DOOR_DONE](
        context: ActionContext<State, State>,
        day: number
    ) {
        context.commit(mutationTypes.MARK_DOOR_DONE, day);
        await context.dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.SET_SCALE_FACTOR](
        context: ActionContext<State, State>,
        scaleFactor: number
    ) {
        context.commit(mutationTypes.SET_SCALE_FACTOR, scaleFactor);
        await context.dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.GET_AUTH_HEADERS](context: ActionContext<State, State>) {
        context.commit(mutationTypes.SET_AUTH_HEADERS, await getAxiosConfig());
    },
    async [actionTypes.GET_CALENDAR](context: ActionContext<State, State>) {
        if (state.displayPeriod > 0) {
            return;
        }
        await context.dispatch(actionTypes.GET_AUTH_HEADERS);
        const response = await axios.get<UserData>(
            "/api/calendars",
            context.state.axiosConfig
        );
        context.commit(mutationTypes.SET_USER_DATA, response.data);
        context.commit(
            mutationTypes.SET_DISPLAY_PERIOD,
            response.data.latestPeriod
        );
    },
    async [actionTypes.SET_CALENDAR](context: ActionContext<State, State>) {
        await axios.put<UserData>(
            "/api/calendars",
            context.state.userData,
            context.state.axiosConfig
        );
    },
    async [actionTypes.RESET_CALENDAR](context: ActionContext<State, State>) {
        const response = await axios.post<UserData>(
            "/api/calendars",
            context.state.userData,
            context.state.axiosConfig
        );
        context.commit(mutationTypes.SET_USER_DATA, response.data);
        context.commit(
            mutationTypes.SET_DISPLAY_PERIOD,
            response.data.latestPeriod
        );
    },
    [actionTypes.SET_DISPLAY_PERIOD](
        context: ActionContext<State, State>,
        period: number
    ) {
        context.commit(mutationTypes.SET_DISPLAY_PERIOD, period);
    },
    async [actionTypes.ENABLE_SHARED_LINK](
        context: ActionContext<State, State>
    ) {
        const response = await axios.post<SharedLinkPostRequest,{data: UserData}>(
            "/api/sharedCalendars",
            { period: context.getters.displayPeriod },
            context.state.axiosConfig
        );
        context.commit(mutationTypes.SET_USER_DATA, response.data);
    },
    async [actionTypes.DISABLE_SHARED_LINK](context: ActionContext<State,State>){
        const linkId = context.getters.sharedLinkId.fields[0];
        const _ = await axios.delete(
            "/api/sharedCalendars/" + linkId,
            context.state.axiosConfig
        );
        context.commit(mutationTypes.REMOVE_SHARED_LINK, context.getters.displayPeriod);
    }
};

export const store = createStore({
    state,
    mutations,
    actions,
    plugins: [createLogger()],
    getters: {
        displayCalendar: (state: State) => {
            return state.userData.calendars[state.displayPeriod];
        },
        periods: (state: State) => {
            return Object.keys(state.userData.calendars)
                .map((key) => Number(key))
                .sort()
                .reverse();
        },
        displayPeriod: (state: State) => {
            return state.displayPeriod;
        },
        latestPeriod: (state: State) => {
            return state.userData.latestPeriod;
        },
        sharedLinkId: (state: State) => {
            return state.userData.calendars[state.displayPeriod].settings.sharedLinkId;
        },
        sharedLinkValue: (state: State) => {
            const linkId = state.userData.calendars[state.displayPeriod].settings.sharedLinkId?.fields[0];
            return `https://${window.location.host}/s/${linkId}`;
        },
    },
});
