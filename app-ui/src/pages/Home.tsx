import React, {useEffect} from 'react';
import {DataGrid, GridCellParams, GridFilterModel, GridRowParams, GridSortModel} from '@mui/x-data-grid';
import {Box, Button, Stack} from "@mui/material";
import {Track} from "../models/demoData/Track";
import {useService} from "../ContainerContext";
import {ApiCommsService} from "../services/ApiCommsService";
import {ColumnDefs} from "../models/demoData/ColumnDefs";
import {ColumnDef} from "../MUIDataGridSupport/ColumnDef";
import {PaginationState} from "../MUIDataGridSupport/PaginationState";
import {ODataQueryBuilderForMuiDataGrid} from "../MUIDataGridSupport/ODataQueryBuilderForMuiDataGrid";
import {MuiDataGridState} from "../MUIDataGridSupport/MuiDataGridState";
import {AppNotificationKind} from "../models/AppNotificationKind";

const Home = () => {

    const recreateDatabase = () => {
        setDbConnected(false);
        setTrackCount(0);
        setTracks([]);
        apiComms.reCreateDemoDb().then().catch(error => console.error(error));
    };

    const cellClicked = (p: GridCellParams<Track>) => {
        if (p.row.album.value)
            setCurrentAlbumArtwork(p.row.album.value.inlineImage);
    };

    const rowClicked = (p: GridRowParams<Track>) => {
        if (p.row.album.value)
            setCurrentAlbumArtwork(p.row.album.value.inlineImage);
    }

    const [apiReady, setApiReady] = React.useState(false);
    const [dbConnected, setDbConnected] = React.useState(false);
    const [tracks, setTracks] = React.useState<Track[]>([]);
    const [trackCount, setTrackCount] = React.useState<number>(0);
    const [tracksLoading, setTracksLoading] = React.useState(false);
    const [currentAlbumArtwork, setCurrentAlbumArtwork] = React.useState<string>('');

    const [paginationModel, setPaginationModel] = React.useState<PaginationState>(new PaginationState());
    const [sortModel, setSortModel] = React.useState<GridSortModel>([]);
    const [filterModel, setFilterModel] = React.useState<GridFilterModel>({items: []});
    const [colDefs, setColDefs] = React.useState<ColumnDef<Track>[]>(ColumnDefs.getTrackColumnDefs());

    const apiComms = useService(ApiCommsService);

    apiComms.OnAppNotification.subscribe(notification => {
        switch (notification.kind) {
            case AppNotificationKind.ApiInitialized:
                setApiReady(true);
                break;
            case AppNotificationKind.DatabaseConnected:
                setDbConnected(true);
                break;
            default:
                break;
        }
    });

    const buildODataQuery = () => {
        const gridState = new MuiDataGridState<Track>(filterModel, sortModel, paginationModel, colDefs);
        return ODataQueryBuilderForMuiDataGrid.BuildODataQueryForMuiDataGrid<Track>(gridState);
    }

    const loadTracks = async () => {
        if (!dbConnected) return;
        const odataQuery = buildODataQuery();
        const queryResult = await apiComms.getTracks(odataQuery);
        setTrackCount(queryResult.count ?? 0);
        setTracks(queryResult.entities);
    }

    useEffect(() => {
        setTracksLoading(true);
        loadTracks().then().finally(() => setTracksLoading(false));
    }, [paginationModel, sortModel, filterModel, dbConnected]);

    return (
        <Box sx={{ width: '100%', height: '100%' }}>
            <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button size="small" disabled={!apiReady} onClick={recreateDatabase}>
                    Recreate Database
                </Button>
            </Stack>
            <div style={{ display: 'flex', flexDirection: 'column', maxHeight: '50%', marginTop: '1rem' }}>
                <DataGrid
                    columns={colDefs}
                    rows={tracks}
                    pageSizeOptions={[5, 10, 25]}
                    rowCount={trackCount}
                    paginationModel={paginationModel}
                    sortModel={sortModel}
                    filterModel={filterModel}
                    sortingMode={'server'}
                    filterMode={'server'}
                    paginationMode={'server'}
                    onPaginationModelChange={setPaginationModel}
                    onSortModelChange={setSortModel}
                    onFilterModelChange={setFilterModel}
                    onCellClick={cellClicked}
                    onRowClick={rowClicked}
                    loading={tracksLoading} />
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', marginTop: '1rem', justifyItems: 'center', alignItems: 'center' }}>
                <img src={currentAlbumArtwork} alt={'Album Artwork'} hidden={!currentAlbumArtwork} height={300} width={300}></img>
            </div>
        </Box>
    );
}

export default Home;
