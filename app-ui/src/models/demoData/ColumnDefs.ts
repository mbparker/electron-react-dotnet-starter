import {GridColDef, GridValidRowModel} from '@mui/x-data-grid';
import {Track} from "./Track";
import {LazyShim} from "../LazyShim";
import {NamedEntity} from "./NamedEntity";

export type ColumnDef<T extends GridValidRowModel> = GridColDef<T> & { nestedField?: string };

export class ColumnDefs {

    public static getTrackColumnDefs(): ColumnDef<Track>[] {
        return [
            {
                field: 'genre',
                headerName: 'Genre',
                valueGetter: (v: LazyShim<NamedEntity>) => v.value?.name ?? '',
                width: 150,
                nestedField: 'genre.value.name',
            },
            {
                field: 'artist',
                headerName: 'Artist',
                valueGetter: (v: LazyShim<NamedEntity>) => v.value?.name ?? '',
                width: 150,
                nestedField: 'artist.value.name'
            },
            {
                field: 'album',
                headerName: 'Album',
                valueGetter: (v: LazyShim<NamedEntity>) => v.value?.name ?? '',
                width: 150,
                nestedField: 'album.value.name'
            },
            {
                field: 'discNumber',
                headerName: 'Disc',
                width: 100
            },
            {
                field: 'trackNumber',
                headerName: 'Number',
                width: 100
            },
            {
                field: 'name',
                headerName: 'Title',
                width: 350
            }
        ];
    }
}
