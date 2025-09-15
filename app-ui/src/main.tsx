import "reflect-metadata";
import {container} from "tsyringe";
import ReactDOM from "react-dom/client";
import {HashRouter} from "react-router-dom";
import { Provider } from "react-redux";
import { persistor, store} from "./stores/store";
import Router from "./router";
import "./assets/css/app.css";
import ContainerContext from "./ContainerContext";
import {ContainerRegistration} from "./ContainerRegistration";
import {AppInit} from "./AppInit";
import ProgressModal from "./components/Modal/ProgressModal";
import { PersistGate } from 'redux-persist/integration/react'
import {CssBaseline, ThemeProvider} from "@mui/material";
import { createTheme } from '@mui/material/styles';

if (ContainerRegistration.RegisterDependencies()) {
    const appInit = container.resolve<AppInit>(AppInit);
    appInit.InitApp();
}

const theme = createTheme({
    colorSchemes: {
        light: true,
        dark: true,
    },
});

ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
    <ContainerContext.Provider value={container}>
        <Provider store={store}>
            <PersistGate loading={null} persistor={persistor}>
                <HashRouter>
                    <ThemeProvider theme={theme} noSsr={true}>
                        <CssBaseline />
                        <ProgressModal />
                        <Router />
                    </ThemeProvider>
                </HashRouter>
            </PersistGate>
        </Provider>
    </ContainerContext.Provider>
);
