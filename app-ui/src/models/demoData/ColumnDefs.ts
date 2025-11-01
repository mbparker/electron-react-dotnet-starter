import {GridColDef, GridValidRowModel} from '@mui/x-data-grid';
import {Track} from "./Track";
import {LazyShim} from "../LazyShim";
import {NamedEntity} from "./NamedEntity";
import {Album} from "./Album";

export type ColumnDef<T extends GridValidRowModel> = GridColDef<T> & { nestedField?: string };

export class ColumnDefs {

    public static getTrackColumnDefs(): ColumnDef<Track>[] {
        return [
            {
                field: 'genre',
                headerName: 'Genre',
                valueGetter: (v: LazyShim<NamedEntity>) => v?.value?.name ?? '',
                width: 150,
                type: 'string',
                nestedField: 'genre.value.name',
            },
            {
                field: 'artist',
                headerName: 'Artist',
                valueGetter: (v: LazyShim<NamedEntity>) => v?.value?.name ?? '',
                width: 150,
                type: 'string',
                nestedField: 'artist.value.name'
            },
            {
                field: 'album',
                headerName: 'Album',
                valueGetter: (v: LazyShim<NamedEntity>) => v?.value?.name ?? '',
                width: 150,
                type: 'string',
                nestedField: 'album.value.name'
            },
            {
                field: 'album',
                headerName: 'Release Date',
                valueGetter: (v: LazyShim<Album>) => {
                    const val = v?.value?.releaseDate;
                    if (val) return new Date(val);
                    return undefined
                },
                width: 150,
                type: 'date',
                nestedField: 'album.value.releaseDate'
            },
            {
                field: 'discNumber',
                headerName: 'Disc',
                width: 100,
                type: 'number'
            },
            {
                field: 'trackNumber',
                headerName: 'Number',
                width: 100,
                type: 'number'
            },
            {
                field: 'name',
                headerName: 'Title',
                width: 350,
                type: 'string'
            }
        ];
    }
}
