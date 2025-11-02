import {AppNotificationKind} from "./AppNotificationKind";

export class AppNotification {
    public kind: AppNotificationKind = AppNotificationKind.Unknown;
    public eventData: any;
}
