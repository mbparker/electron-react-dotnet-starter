import {FilterBuilder, ODataQueryBuilder, ODataQueryOptions, OrderDirection} from "../utils/ODataQueryBuilder";
import {GridFilterItem, GridValidRowModel} from "@mui/x-data-grid";
import {ColumnDef} from "./ColumnDef";
import {MuiDataGridState} from "./MuiDataGridState";

export class ODataQueryBuilderForMuiDataGrid<T extends GridValidRowModel> {

    public static BuildODataQueryForMuiDataGrid<T extends GridValidRowModel>(
        gridState: MuiDataGridState<T>): string {
        const instance = new ODataQueryBuilderForMuiDataGrid<T>(gridState);
        return instance.buildODataQueryString();
    }

    private constructor(private readonly gridState: MuiDataGridState<T>) {
    }

    private buildODataQueryString() {
        const odataOptions: ODataQueryOptions = {
            orderBy: this.buildSorting(),
            filter: this.buildFiltering(),
            count: true, // causes an extra query, but necessary for paging.
            skip: this.gridState.pagination.page * this.gridState.pagination.pageSize,
            top: this.gridState.pagination.pageSize
        };
        return ODataQueryBuilder.buildQuery(odataOptions);
    }

    private getColumnDef(gridFieldName: string) : ColumnDef<T> {
        const col = this.gridState.columnDefs.filter(x => x.field == gridFieldName).pop();
        if (col) return col;
        throw new Error(`Column definition not found: ${gridFieldName}`);
    }

    private getEffectiveFieldName(gridFieldName: string) {
        const columnDef = this.getColumnDef(gridFieldName);
        return columnDef.nestedField ?? columnDef.field;
    }

    private buildSorting() {
        return this.gridState.sorting.map((item) => {
            return {
                property: this.getEffectiveFieldName(item.field),
                direction: item.sort == 'asc' ? OrderDirection.Ascending : OrderDirection.Descending
            };
        });
    }

    private buildFiltering() {
        let exps = this.gridState.filter.items.map((item) => {
            if (!item.value) return undefined;
            switch (item.operator) {
                case 'contains':
                    return FilterBuilder.contains(this.getEffectiveFieldName(item.field), item.value);
                case 'doesNotContain':
                    return FilterBuilder.not(FilterBuilder.contains(this.getEffectiveFieldName(item.field), item.value));
                case 'startsWith':
                    return FilterBuilder.startsWith(this.getEffectiveFieldName(item.field), item.value);
                case 'endsWith':
                    return FilterBuilder.endsWith(this.getEffectiveFieldName(item.field), item.value);
                case 'equals':
                case '=':
                case 'is':
                    return FilterBuilder.eq(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValueToLiteralExpression(item));
                case 'doesNotEqual':
                case '!=':
                case 'not':
                    return FilterBuilder.ne(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValueToLiteralExpression(item));
                case '>':
                case 'after':
                    return FilterBuilder.gt(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValueToLiteralExpression(item));
                case '<':
                case 'before':
                    return FilterBuilder.lt(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValueToLiteralExpression(item));
                case '>=':
                case 'onOrAfter':
                    return FilterBuilder.ge(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValueToLiteralExpression(item));
                case '<=':
                case 'onOrBefore':
                    return FilterBuilder.le(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValueToLiteralExpression(item));
                case 'isEmpty':
                    return FilterBuilder.eq(FilterBuilder.property(this.getEffectiveFieldName(item.field)), FilterBuilder.null());
                case 'isNotEmpty':
                    return FilterBuilder.ne(FilterBuilder.property(this.getEffectiveFieldName(item.field)), FilterBuilder.null());
                case 'isAnyOf':
                    return FilterBuilder.isAnyOf(FilterBuilder.property(this.getEffectiveFieldName(item.field)), this.resolveValuesToLiteralExpressions(item));
                default:
                    return undefined;
            }
        });

        exps = exps.filter(x => !!x);

        // the free version of the MUI DataGrid only supports a single filter.
        // the filter expressions could be joined here, returning a single binary expression at the end.
        return exps.pop();
    }

    private resolveValuesToLiteralExpressions(filterItem: GridFilterItem) {
        const col = this.getColumnDef(filterItem.field);
        if (Array.isArray(filterItem.value)) {
            return filterItem.value.map((valueItem: any) => this.getLiteralExpForValue(col, valueItem));
        }
        return [this.getLiteralExpForValue(col, filterItem.value)];
    }

    private resolveValueToLiteralExpression(filterItem: GridFilterItem) {
        return this.resolveValuesToLiteralExpressions(filterItem)[0];
    }

    private getLiteralExpForValue(col: ColumnDef<T>, value: any) {
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

}
