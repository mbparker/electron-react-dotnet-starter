import {
    OpenDialogOptions,
    OpenDialogReturnValue,
    MessageBoxOptions,
    MessageBoxReturnValue,
    SaveDialogOptions, SaveDialogReturnValue
} from "electron";
import {injectable} from "tsyringe";

@injectable()
export class ElectronApiService {
    private readonly appApi: any;
    private _reloading: boolean = false;

    public constructor() {
        this.appApi = (window as any).appApi;
    }

    public get reloading(): boolean {
        return this._reloading;
    }

    public async showNativeMessageBox(options: MessageBoxOptions): Promise<MessageBoxReturnValue> {
        return await new Promise<MessageBoxReturnValue>((resolve) => {
            this.appApi.onShowMessageBox((result: MessageBoxReturnValue) => resolve(result));
            this.appApi.showMessageBox(options);
        });
    }

    public async showNativeOpenDialog(options: OpenDialogOptions): Promise<OpenDialogReturnValue> {
        return await new Promise<OpenDialogReturnValue>((resolve) => {
            this.appApi.onShowOpenDialog((result: OpenDialogReturnValue) => resolve(result));
            this.appApi.showOpenDialog(options);
        });
    }

    public async showNativeSaveDialog(options: SaveDialogOptions): Promise<SaveDialogReturnValue> {
        return await new Promise<SaveDialogReturnValue>((resolve) => {
            this.appApi.onShowSaveDialog((result: SaveDialogReturnValue) => resolve(result));
            this.appApi.showSaveDialog(options);
        });
    }

    public async resolveFileUrl(path: string): Promise<string> {
        return await new Promise<string>((resolve) => {
            this.appApi.onResolveFileUrl((result: string) => resolve(result));
            this.appApi.resolveFileUrl(path);
        });
    }

    public async openUrlExternal(protocolUrl: string): Promise<void> {
        await new Promise<void>((resolve) => {
            this.appApi.openUrlExternal(protocolUrl);
            resolve();
        });
    }

    public async quitApp(): Promise<void> {
        await new Promise<void>((resolve) => {
            this.appApi.performAppQuit();
            resolve();
        });
    }

    public handleReloadDocument(): void {
        this.appApi.onReloadClicked(() => {
            this._reloading = true;
            this.appApi.performReload();
        });
    }

    public async getApiUrl(): Promise<string> {
        return await new Promise<string>((resolve) => {
            this.appApi.onGetApiUrl((result: string) => resolve(result));
            this.appApi.getApiUrl();
        });
    }
}
