import {Track} from "./Track";
import {Utils} from "../../utils/Utils";
import {ColumnDef} from "../../MUIDataGridSupport/ColumnDef";

export class ColumnDefs {

    public static getTrackColumnDefs(): ColumnDef<Track>[] {
        return [
            {
                field: 'genreName',
                headerName: 'Genre',
                valueGetter: (v: never, r: Track) => r?.genre?.value?.name,
                width: 150,
                type: 'string',
                nestedField: 'genre.value.name',
            },
            {
                field: 'artistName',
                headerName: 'Artist',
                valueGetter: (v: never, r: Track) => r?.artist?.value?.name,
                width: 150,
                type: 'string',
                nestedField: 'artist.value.name'
            },
            {
                field: 'albumName',
                headerName: 'Album',
                valueGetter: (v: never, r: Track) => r?.album?.value?.name,
                width: 150,
                type: 'string',
                nestedField: 'album.value.name'
            },
            {
                field: 'albumReleaseDate',
                headerName: 'Release Date',
                valueGetter: (v: never, r: Track) => {
                    let val = r?.album?.value?.releaseDate;
                    if (val) return Utils.getUtcDate(val);
                    return undefined;
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
