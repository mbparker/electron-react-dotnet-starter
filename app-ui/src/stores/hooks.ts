import { TypedUseSelectorHook, useDispatch, useSelector } from "react-redux";
import {RootState} from "./rootReducer";
import {ThunkDispatch, Action} from "@reduxjs/toolkit";

// Use throughout your app instead of plain `useDispatch` and `useSelector`
export type AppThunkDispatch = ThunkDispatch<RootState, any, Action>
export const useAppDispatch = () => useDispatch<AppThunkDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
