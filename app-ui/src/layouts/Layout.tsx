import React from 'react';
import { Outlet } from "react-router-dom"
import {Box} from "@mui/material";

const Layout = () => {
    return (
        <Box sx={{ width: 'calc(100dvw - 2rem)', height: 'calc(100dvh - 2rem)', margin: '1rem' }}>
            <Outlet/>
        </Box>
    );
}

export default Layout;
