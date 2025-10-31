// Since the API wraps navigation props in Lazy<T>, which injects an extra "Value" property, this class recreates that layer
// to keep things consistent on this end.
export class LazyShim<T> {
    public value: T | undefined;
    public constructor(value?: T) {
        this.value = value;
    }
}
