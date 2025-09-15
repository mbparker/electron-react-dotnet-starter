import {persistReducer, persistStore} from "redux-persist";
import {PersistConfig} from "redux-persist/es/types";
import storage from "redux-persist/lib/storage";
import rootReducer from "./rootReducer";
import {configureStore, Reducer} from "@reduxjs/toolkit";

const persistConfig: PersistConfig<unknown> = {
    key: 'datastore',
    storage
}

export const store = configureStore({
    reducer: persistReducer(persistConfig, rootReducer() as Reducer),
    devTools: process.env.NODE_ENV !== 'production'
});

export const persistor = persistStore(store)
