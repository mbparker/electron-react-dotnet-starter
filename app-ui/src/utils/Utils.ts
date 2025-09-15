export class Utils {

    public static sleep(ms: number): Promise<void> {
        return new Promise<void>((res) => {
            setTimeout(() => { res(); }, ms);
        })
    }

}
