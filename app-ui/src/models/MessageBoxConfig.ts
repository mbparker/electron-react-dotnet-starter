export enum MessageBoxType {
    None,
    Info,
    Error,
    Question,
    Warning
}

export enum MessageBoxButton {
    None,
    Ok,
    Cancel,
    Yes,
    No,
    Abort,
    Retry
}

export class MessageBoxConfig {

    public constructor(
        public type: MessageBoxType,
        public title: string,
        public message: string,
        public buttons: MessageBoxButton[],
        public defaultButton: MessageBoxButton,
        public cancelButton: MessageBoxButton) {
    }

}

export class MessageBoxHelpers {

    public static getButtonText(button: MessageBoxButton): string | undefined {
        switch(button) {
            case MessageBoxButton.Ok:
                return 'OK';
            case MessageBoxButton.Yes:
                return 'Yes';
            case MessageBoxButton.No:
                return 'No';
            case MessageBoxButton.Cancel:
                return 'Cancel';
            case MessageBoxButton.Abort:
                return 'Abort';
            case MessageBoxButton.Retry:
                return 'Retry';
        }
        return undefined;
    }

}
