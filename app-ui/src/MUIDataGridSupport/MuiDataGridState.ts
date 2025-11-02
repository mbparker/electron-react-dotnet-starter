import {GridFilterModel, GridSortModel, GridValidRowModel} from "@mui/x-data-grid";
import {PaginationState} from "./PaginationState";
import {ColumnDef} from "./ColumnDef";

export class MuiDataGridState<T extends GridValidRowModel> {
    public constructor(
        public readonly filter: GridFilterModel,
        public readonly sorting: GridSortModel,
        public readonly pagination: PaginationState,
        public readonly columnDefs: ColumnDef<T>[]) {
    }
}
