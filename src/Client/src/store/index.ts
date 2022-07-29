import { Auth0Client } from "@auth0/auth0-spa-js";
import axios from "axios";
import { inject } from "vue";
import { ActionContext, createLogger, createStore } from "vuex";
import {
    Calendar,
    DisplayType,
    emptyCalendar,
    emptyUserData,
    SharedLinkPostRequest,
    SharedLinkResponse,
    UserData,
} from "../models/calendar";
import { FOption, getSome, isSome, None, Some } from "../models/fsharp-helpers";
import * as actionTypes from "./action-types";
import * as mutationTypes from "./mutation-types.";

export interface State {
    userData: UserData;
    displayPeriod: number;
    axiosConfig: any;
    loading: Boolean;
    currentSharedCalendar: [string,FOption<SharedLinkResponse>];
}

const state: State = {
    userData: emptyUserData(),
    displayPeriod: 0,
    axiosConfig: null,
    loading: false,
    currentSharedCalendar: ["",Some({
        calendar: emptyCalendar(),
        period: 0,
        displayName: None<string>(),
    })],
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
    [mutationTypes.SET_DISPLAY_NAME](state: State, displayName: string) {
        state.userData.displayName = Some<string>(displayName);
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
    [mutationTypes.SET_DISPLAY_TYPE](state: State, displayType: DisplayType) {
        state.userData.displayType = {case: DisplayType[displayType]};
    },
    [mutationTypes.SET_USER_DATA](state: State, userData: UserData) {
        state.userData = userData;
    },
    [mutationTypes.REMOVE_SHARED_LINK](state: State, period: number) {
        state.userData.calendars[period]!.settings.sharedLinkId = undefined;
    },
    [mutationTypes.SET_LOADING](state: State, loading: Boolean) {
        state.loading = loading;
    },
    [mutationTypes.SET_CURRENT_SHARED_CALENDAR](
        state: State,
        payload: [string, FOption<SharedLinkResponse>]
    ) {
        state.currentSharedCalendar = payload;
    },
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
    async [actionTypes.SET_DISPLAY_NAME](
        context: ActionContext<State, State>,
        displayName: string
    ) {
        context.commit(mutationTypes.SET_DISPLAY_NAME, displayName);
        await context.dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.SET_SCALE_FACTOR](
        context: ActionContext<State, State>,
        scaleFactor: number
    ) {
        context.commit(mutationTypes.SET_SCALE_FACTOR, scaleFactor);
        await context.dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.SET_DISPLAY_TYPE](
        context: ActionContext<State, State>,
        displayType: DisplayType
    ) {
        context.commit(mutationTypes.SET_DISPLAY_TYPE, displayType);
        await context.dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.GET_AUTH_HEADERS](context: ActionContext<State, State>) {
        context.commit(mutationTypes.SET_AUTH_HEADERS, await getAxiosConfig());
    },
    async [actionTypes.GET_CALENDAR](context: ActionContext<State, State>) {
        if (state.displayPeriod > 0) {
            return;
        }
        context.commit(mutationTypes.SET_LOADING, true);
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
        context.commit(mutationTypes.SET_LOADING, false);
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
        const response = await axios.post<
            SharedLinkPostRequest,
            { data: UserData }
        >(
            "/api/sharedCalendars",
            { period: context.getters.displayPeriod },
            context.state.axiosConfig
        );
        context.commit(mutationTypes.SET_USER_DATA, response.data);
    },
    async [actionTypes.DISABLE_SHARED_LINK](
        context: ActionContext<State, State>
    ) {
        const linkId = context.getters.sharedLinkId.fields[0];
        const _ = await axios.delete(
            "/api/sharedCalendars/" + linkId,
            context.state.axiosConfig
        );
        context.commit(
            mutationTypes.REMOVE_SHARED_LINK,
            context.getters.displayPeriod
        );
    },
    async [actionTypes.GET_SHARED_CALENDAR](
        context: ActionContext<State, State>,
        sharedLinkId: string
    ) {
        if(sharedLinkId === context.state.currentSharedCalendar[0]) {
            return;
        }
        context.commit(mutationTypes.SET_LOADING, true);
        const response = await axios.get<SharedLinkResponse>(
            "/api/sharedCalendars/" + sharedLinkId
        , {validateStatus: (status) => {return status === 200 || status === 404}});

            const data = response.status === 200 ? Some(response.data) : None();
            context.commit(
                mutationTypes.SET_CURRENT_SHARED_CALENDAR,
                [sharedLinkId,data]
            );
            context.commit(mutationTypes.SET_LOADING, false);


    },
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
        displayType: (state: State) => {
            if(!!state.userData.displayType) {
                return DisplayType[state.userData.displayType.case as keyof typeof DisplayType];
            }
            return DisplayType.Door;
        },
        displayName: (state: State) => {
            return isSome(state.userData.displayName)
                ? getSome(state.userData.displayName)
                : "";
        },
        latestPeriod: (state: State) => {
            return state.userData.latestPeriod;
        },
        sharedLinkId: (state: State) => {
            return state.userData.calendars[state.displayPeriod].settings
                .sharedLinkId;
        },
        sharedLinkValue: (state: State) => {
            const linkId =
                state.userData.calendars[state.displayPeriod].settings
                    .sharedLinkId?.fields[0];
            return `${window.location.protocol}//${window.location.host}/#/s/${linkId}`;
        },
    },
});
