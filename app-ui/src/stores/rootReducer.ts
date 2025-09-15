import {CombinedState} from "redux";
import {AnyAction, combineReducers} from "@reduxjs/toolkit";
import pageLoaderReducer, {PageLoaderState} from "./pageLoaderSlice";

export type RootState = CombinedState<{
    pageLoaderReducer: PageLoaderState
}>

const staticReducers = {
    pageLoaderReducer
}

const rootReducer =
    () =>
        (state: RootState, action: AnyAction) => {
            const combinedReducer = combineReducers({
                ...staticReducers,
            })
            return combinedReducer(state, action)
        }

export default rootReducer
