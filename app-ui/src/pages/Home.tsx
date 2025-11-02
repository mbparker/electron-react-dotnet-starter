import React, {useEffect} from 'react';
import {DataGrid, GridFilterItem, GridFilterModel, GridSortModel} from '@mui/x-data-grid';
import {Box, Button, Stack} from "@mui/material";
import {Track} from "../models/demoData/Track";
import {useService} from "../ContainerContext";
import {ApiCommsService} from "../services/ApiCommsService";
import {Utils} from "../utils/Utils";
import {ColumnDefs} from "../models/demoData/ColumnDefs";
import {
    FilterBuilder,
    FilterExpression,
    ODataQueryBuilder,
    ODataQueryOptions,
    OrderByClause,
    OrderDirection
} from "../utils/ODataQueryBuilder";
import {ColumnDef} from "../MUIDataGridSupport/ColumnDef";

const Home = () => {

    const removeRow = () => {};
    const addRow = () => {};

    const [tracks, setTracks] = React.useState<Track[]>([]);
    const [trackCount, setTrackCount] = React.useState<number | undefined | null>();
    const [tracksLoading, setTracksLoading] = React.useState(false);

    const [paginationModel, setPaginationModel] = React.useState({
        page: 0,
        pageSize: 10,
    });
    const [sortModel, setSortModel] = React.useState<GridSortModel>([]);
    const [filterModel, setFilterModel] = React.useState<GridFilterModel>({
        items: [],
    });
    const [colDefs, setColDefs] = React.useState<ColumnDef<Track>[]>(ColumnDefs.getTrackColumnDefs());
    const [firstRender, setFirstRender] = React.useState(true);

    const apiComms = useService(ApiCommsService);

    const getColumnDef = (field: string) => {
        const col = colDefs.filter(x => x.field == field).pop();
        if (col) return col;
        throw new Error(`Column definition not found: ${field}`);
    }

    const getEffectiveField = (field: string) => {
        return getColumnDef(field).nestedField ?? field;
    }

    const resolveValuesToLiteralExpressions = (filterItem: GridFilterItem) => {
        const col = getColumnDef(filterItem.field);
        if (Array.isArray(filterItem.value)) {
            return filterItem.value.map((valueItem: any) => getLiteralExpForValue(col, valueItem));
        }
        return [getLiteralExpForValue(col, filterItem.value)];
    }

    const resolveValueToLiteralExpression = (filterItem: GridFilterItem) => {
        return resolveValuesToLiteralExpressions(filterItem)[0];
    }

    const getLiteralExpForValue = (col: ColumnDef<Track>, value: any) => {
        switch (col.type) {
            case 'string':
                return FilterBuilder.string(value);
            case 'number':
                return FilterBuilder.number(value);
            case 'boolean':
                return FilterBuilder.boolean(value);
            case 'dateTime':
            case 'date':
                return FilterBuilder.datetime(value);
            default:
                throw new Error(`Column type not supported: ${col.type}`);
        }
    }

    const getODataSorting = () : OrderByClause[] => {
        return sortModel.map((item) => {
            return {
                property: getEffectiveField(item.field),
                direction: item.sort == 'asc' ? OrderDirection.Ascending : OrderDirection.Descending
            };
        });
    }

    const getODataFilter = () : FilterExpression | undefined => {

        let exps = filterModel.items.map((item) => {
            if (!item.value) return undefined;
            switch (item.operator) {
                case 'contains':
                    return FilterBuilder.contains(getEffectiveField(item.field), item.value);
                case 'doesNotContain':
                    return FilterBuilder.not(FilterBuilder.contains(getEffectiveField(item.field), item.value));
                case 'startsWith':
                    return FilterBuilder.startsWith(getEffectiveField(item.field), item.value);
                case 'endsWith':
                    return FilterBuilder.endsWith(getEffectiveField(item.field), item.value);
                case 'equals':
                case '=':
                case 'is':
                    return FilterBuilder.eq(FilterBuilder.property(getEffectiveField(item.field)), resolveValueToLiteralExpression(item));
                case 'doesNotEqual':
                case '!=':
                case 'not':
                    return FilterBuilder.ne(FilterBuilder.property(getEffectiveField(item.field)), resolveValueToLiteralExpression(item));
                case '>':
                case 'after':
                    return FilterBuilder.gt(FilterBuilder.property(getEffectiveField(item.field)), resolveValueToLiteralExpression(item));
                case '<':
                case 'before':
                    return FilterBuilder.lt(FilterBuilder.property(getEffectiveField(item.field)), resolveValueToLiteralExpression(item));
                case '>=':
                case 'onOrAfter':
                    return FilterBuilder.ge(FilterBuilder.property(getEffectiveField(item.field)), resolveValueToLiteralExpression(item));
                case '<=':
                case 'onOrBefore':
                    return FilterBuilder.le(FilterBuilder.property(getEffectiveField(item.field)), resolveValueToLiteralExpression(item));
                case 'isEmpty':
                    return FilterBuilder.eq(FilterBuilder.property(getEffectiveField(item.field)), FilterBuilder.null());
                case 'isNotEmpty':
                    return FilterBuilder.ne(FilterBuilder.property(getEffectiveField(item.field)), FilterBuilder.null());
                case 'isAnyOf':
                    return FilterBuilder.isAnyOf(FilterBuilder.property(getEffectiveField(item.field)), resolveValuesToLiteralExpressions(item));
                default:
                    return undefined;
            }
        });

        exps = exps.filter(x => !!x);

        // the free version of the MUI DataGrid only supports a single filter.
        // the filter expressions could be joined here, returning a single binary expression at the end.
        return exps.pop();
    }

    const buildODataQuery = () => {
        const odataOptions: ODataQueryOptions = {
            orderBy: getODataSorting(),
            filter: getODataFilter(),
            count: true,
            skip: paginationModel.page * paginationModel.pageSize,
            top: paginationModel.pageSize
        };
        return ODataQueryBuilder.buildQuery(odataOptions);
    }

    const loadTracks = async () => {
        if (firstRender) {
            while (!apiComms.isConnected || !await apiComms.isDbConnected()) {
                await Utils.sleep(100);
            }
            setFirstRender(false);
        }
        console.log('Filter Model', filterModel);
        const odataQuery = buildODataQuery();
        console.log('OData Query', odataQuery);
        const queryResult = await apiComms.getTracks(odataQuery);
        setTrackCount(queryResult.count);
        setTracks(queryResult.entities.map(x => {
            return {...x, albumReleaseDate: x.album};
        }));
    }

    useEffect(() => {
        setTracksLoading(true);
        loadTracks().then().finally(() => setTracksLoading(false));
    }, [paginationModel, sortModel, filterModel]);

    return (
        <Box sx={{ width: '100%', height: '100%' }}>
            <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Button size="small" onClick={removeRow}>
                    Remove
                </Button>
                <Button size="small" onClick={addRow}>
                    Add
                </Button>
            </Stack>
            <div style={{ display: 'flex', flexDirection: 'column', maxHeight: '50%', marginTop: '1rem' }}>
                <DataGrid
                    columns={colDefs}
                    rows={tracks}
                    pageSizeOptions={[5, 10, 25]}
                    rowCount={trackCount ?? 0}
                    paginationModel={paginationModel}
                    sortModel={sortModel}
                    filterModel={filterModel}
                    sortingMode={'server'}
                    filterMode={'server'}
                    paginationMode={'server'}
                    onPaginationModelChange={setPaginationModel}
                    onSortModelChange={setSortModel}
                    onFilterModelChange={setFilterModel}
                    loading={tracksLoading} />
            </div>
        </Box>
    );
}

export default Home;
