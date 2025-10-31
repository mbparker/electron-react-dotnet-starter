export class ODataQueryResult<T> {
    public count: number | undefined | null;
    public entities: T[] = [];
}
