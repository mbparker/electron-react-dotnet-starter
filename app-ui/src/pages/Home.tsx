import React from 'react';
import {DataGrid} from '@mui/x-data-grid';
import {useDemoData} from '@mui/x-data-grid-generator';
import {Box, Button, Stack} from "@mui/material";

const Home = () => {

    const [nbRows, setNbRows] = React.useState(3);
    const removeRow = () => setNbRows((x) => Math.max(0, x - 1));
    const addRow = () => setNbRows((x) => Math.min(100, x + 1));

    const { data, loading } = useDemoData({
        dataSet: 'Commodity',
        rowLength: 100,
        maxColumns: 6,
    });

    return (
        <Box sx={{ width: '100hv', margin: '1rem' }}>
            <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button size="small" onClick={removeRow}>
                    Remove a row
                </Button>
                <Button size="small" onClick={addRow}>
                    Add a row
                </Button>
            </Stack>
            <div style={{ display: 'flex', flexDirection: 'column', maxHeight: '50vh', marginTop: '1rem' }}>
                <DataGrid {...data} rows={data.rows.slice(0, nbRows)} loading={loading} />
            </div>
        </Box>
    );
}

export default Home;
