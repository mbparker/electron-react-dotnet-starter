export class EventEmitter<T extends unknown> {
    private events: ((d: T) => void)[] = [];

    public emit(data: T): void {
        if (this.events.length > 0) {
            this.events.forEach((e => e(data)));
        }
    }

    public subscribe(handler: (d: T) => void): void {
        if (this.events.indexOf(handler) < 0)
            this.events.push(handler);
    }

    public unsubscribe(handler: (d: T) => void): void {
        const index = this.events.indexOf(handler);
        if (index >= 0)
            this.events.splice(index, 1);
    }
}
