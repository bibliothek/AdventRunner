import { Auth0Client } from "@auth0/auth0-spa-js";
import axios from "axios";
import { inject } from "vue";
import { createLogger, createStore, MutationTree } from "vuex";
import { Calendar, emptyCalendar } from "../models/calendar";
import * as actionTypes from "./action-types";
import * as mutationTypes from "./mutation-types.";

export interface State {
    calendar: Calendar;
    axiosConfig: any;
}

const state: State = {
    calendar: emptyCalendar(),
    axiosConfig: null
};

const mutations = {
    [mutationTypes.OPEN_DOOR](state: State, day: number) {
        state.calendar.doors[day - 1].state.case = "Open";
    },
    [mutationTypes.MARK_DOOR_DONE](state: State, day: number) {
        state.calendar.doors[day - 1].state.case = "Done";
    },
    [mutationTypes.SET_CALENDAR](state: State, calendar: Calendar) {
        state.calendar = calendar;
    },
    [mutationTypes.SET_AUTH_HEADERS](state: State, config: any) {
        state.axiosConfig = config;
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
    async [actionTypes.OPEN_DOOR]({ commit, dispatch }, day: number) {
        commit(mutationTypes.OPEN_DOOR, day);
        await dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.MARK_DOOR_DONE]({ commit, dispatch }, day: number) {
        commit(mutationTypes.MARK_DOOR_DONE, day);
        await dispatch(actionTypes.SET_CALENDAR);
    },
    async [actionTypes.GET_AUTH_HEADERS]({ commit }) {
        commit(mutationTypes.SET_AUTH_HEADERS, await getAxiosConfig())
    },
    async [actionTypes.GET_CALENDAR]({ commit, state, dispatch }) {
        if(state.calendar.doors.length > 0) {
            return;
        }
        await dispatch(actionTypes.GET_AUTH_HEADERS);
        const response = await axios.get<Calendar>("/api/calendars", state.axiosConfig);
        commit(mutationTypes.SET_CALENDAR, response.data);
    },
    async [actionTypes.SET_CALENDAR]({ state }) {
        await axios.put<Calendar>(
            "/api/calendars",
            state.calendar,
            state.axiosConfig
        );
    },
};

export const store = createStore({
    state,
    mutations,
    actions,
    plugins: [createLogger()],
});
