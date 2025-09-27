import {combineReducers} from "@reduxjs/toolkit";
import pageLoaderReducer, {PageLoaderState} from "./pageLoaderSlice";

export type RootState = {
    pageLoaderReducer: PageLoaderState
}

const rootReducer = combineReducers({
    pageLoaderReducer
});

export default rootReducer
