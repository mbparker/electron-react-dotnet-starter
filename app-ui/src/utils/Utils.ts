export class Utils {

    public static sleep(ms: number): Promise<void> {
        return new Promise<void>((res) => {
            setTimeout(() => { res(); }, ms);
        })
    }

    public static getUtcDate(date: string | number | Date): Date {
        const dateObj = new Date(date);
        return new Date(dateObj.getUTCFullYear(), dateObj.getUTCMonth(), dateObj.getUTCDate());
    }

}
