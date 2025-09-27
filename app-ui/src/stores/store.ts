import {persistReducer, persistStore} from "redux-persist";
import storage from "redux-persist/lib/storage";
import rootReducer from "./rootReducer";
import {configureStore, Reducer} from "@reduxjs/toolkit";

const persistConfig = {
    key: 'datastore',
    storage
}

export const store = configureStore({
    reducer: persistReducer(persistConfig, rootReducer),
    devTools: process.env.NODE_ENV !== 'production'
});

export const persistor = persistStore(store)
