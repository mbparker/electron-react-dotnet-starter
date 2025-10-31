import {GridColDef} from '@mui/x-data-grid';
import {Track} from "./Track";
import {LazyShim} from "../LazyShim";
import {NamedEntity} from "./NamedEntity";

export class ColumnDefs {

    public static getTrackColumnDefs(): GridColDef<Track>[] {
        return [
            {
                field: 'genre',
                headerName: 'Genre',
                valueGetter: (v: LazyShim<NamedEntity>) => v.value?.name ?? '',
            },
            {
                field: 'artist',
                headerName: 'Artist',
                valueGetter: (v: LazyShim<NamedEntity>) => v.value?.name ?? ''
            },
            {
                field: 'album',
                headerName: 'Album',
                valueGetter: (v: LazyShim<NamedEntity>) => v.value?.name ?? ''
            },
            {
                field: 'discNumber',
                headerName: 'Disc'
            },
            {
                field: 'trackNumber',
                headerName: 'Number'
            },
            {
                field: 'name',
                headerName: 'Title'
            }
        ];
    }
}
