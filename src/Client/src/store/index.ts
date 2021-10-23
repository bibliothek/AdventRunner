import { Auth0Client } from "@auth0/auth0-spa-js";
import axios from "axios";
import { inject } from "vue";
import { createLogger, createStore, MutationTree } from "vuex";
import { Calendar, emptyCalendar } from "../models/calendar";
import calendar from "../views/calendar.vue";
import * as actionTypes from "./action-types";
import * as mutationTypes from "./mutation-types.";

export interface State {
    calendar: Calendar;
}

const state: State = {
    calendar: emptyCalendar(),
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
    async [actionTypes.GET_CALENDAR]({ commit, state }) {
        const config = await getAxiosConfig();
        const response = await axios.get<Calendar>("/api/calendars", config);
        commit(mutationTypes.SET_CALENDAR, response.data);
    },
    async [actionTypes.SET_CALENDAR]({ state }) {
        const config = await getAxiosConfig();
        await axios.put<Calendar>(
            "/api/calendars",
            state.calendar,
            config
        );
    },
};

export const store = createStore({
    state,
    mutations,
    actions,
    plugins: [createLogger()],
});

// export type Mutations<S = State> = {
//     ["OPEN_DOOR"](state: S, payload: number): void,
//     ["MARK_DOOR_COMPLETED"](state: S, payload: number): void,
// }

// export const mutations: MutationTree<State> & Mutations = {
//     ["OPEN_DOOR"](state, payload) {
//         state = {...state, calendar: {...state.calendar, doors: state.calendar.doors.map(x=>x.day === payload ? )}}
//     }
// }
