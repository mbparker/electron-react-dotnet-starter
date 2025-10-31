import React, {useEffect} from 'react';
import {DataGrid} from '@mui/x-data-grid';
import {useDemoData} from '@mui/x-data-grid-generator';
import {Box, Button, Stack} from "@mui/material";
import {Track} from "../models/demoData/Track";
import {useService} from "../ContainerContext";
import {ApiCommsService} from "../services/ApiCommsService";
import {Utils} from "../utils/Utils";

const Home = () => {

    const [nbRows, setNbRows] = React.useState(3);
    const removeRow = () => setNbRows((x) => Math.max(0, x - 1));
    const addRow = () => setNbRows((x) => Math.min(100, x + 1));

    const { data, loading } = useDemoData({
        dataSet: 'Commodity',
        rowLength: 100,
        maxColumns: 6,
        editable: true
    });

    const [tracks, setTracks] = React.useState<Track[]>([]);
    const [trackCount, setTrackCount] = React.useState<number | undefined | null>();
    const [tracksLoading, setTracksLoading] = React.useState(false);

    const apiComms = useService(ApiCommsService);

    const loadTracks = async () => {
        while(!apiComms.isConnected) {
            await Utils.sleep(100);
        }
        const queryResult = await apiComms.getTracks("$count=true");
        console.log(queryResult);
        setTrackCount(queryResult.count);
        setTracks(queryResult.entities);
    }

    useEffect(() => {
        setTracksLoading(true);
        loadTracks().then().finally(() => setTracksLoading(false));
    }, []);

    console.log(data);

    return (
        <Box sx={{ width: '100%', height: '100%' }}>
            <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button size="small" onClick={removeRow}>
                    Remove a row
                </Button>
                <Button size="small" onClick={addRow}>
                    Add a row
                </Button>
            </Stack>
            <div style={{ display: 'flex', flexDirection: 'column', maxHeight: '50%', marginTop: '1rem' }}>
                <DataGrid {...data} rows={data.rows.slice(0, nbRows)} loading={tracksLoading} />
            </div>
        </Box>
    );
}

export default Home;
