import {ApiCommsService} from "./services/ApiCommsService";
import {injectable} from "tsyringe";
import {ElectronApiService} from "./services/ElectronApiService";
import {DialogService} from "./services/DialogService";
import {MessageBoxButton, MessageBoxConfig, MessageBoxType} from "./models/MessageBoxConfig";

@injectable()
export class AppInit {

    private readonly pingMessage: string = 'I can haz cheezbugr??';
    private exitConfirmed: boolean = false;
    private readonly appTitle: string = 'Electron App';

    public constructor(
        private readonly apiComms: ApiCommsService,
        private readonly electronApi: ElectronApiService,
        private readonly dialogService: DialogService) {
    }

    public InitApp(): void {
        this.electronApi.handleReloadDocument();

        window.addEventListener('beforeunload', (e: BeforeUnloadEvent) => {
            if (!this.exitConfirmed) {
                e.preventDefault();
                // noinspection JSDeprecatedSymbols
                e.returnValue = false;
                if (!this.electronApi.reloading)
                    this.handleConfirmShutdown().then();
            }
        }, false);

        this.apiComms.startConnection().then(async () => {
            let resp = await this.apiComms.pingServer({ message: this.pingMessage });
            if (resp.message !== this.pingMessage) {
                throw new Error('Ping response mismatch.');
            }
            await this.apiComms.clientReady();
        }).catch(err => console.error(err));
    }

    private async handleConfirmShutdown(): Promise<void> {
        const result = await this.dialogService.showMessageBox(new MessageBoxConfig(MessageBoxType.Question, this.appTitle, `Are you sure you want to exit ${this.appTitle}?`, [MessageBoxButton.Yes, MessageBoxButton.No], MessageBoxButton.No, MessageBoxButton.No));
        console.log(result);
        if (result === MessageBoxButton.Yes) {
            this.exitConfirmed = true;
            await this.electronApi.quitApp();
        }
    }
}
