import {GridColDef, GridValidRowModel} from "@mui/x-data-grid";

export type ColumnDef<T extends GridValidRowModel> = GridColDef<T> & {
    nestedField?: string
};
