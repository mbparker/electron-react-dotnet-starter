import {injectable} from "tsyringe";
import {MessageBoxButton, MessageBoxConfig} from "../models/MessageBoxConfig";
import {ElectronDialogService} from "./ElectronDialogService";
import {FilenameFilter} from "../models/FilenameFilter";
import {OpenDialogConfig, OpenDialogResult} from "../models/OpenDialogConfig";
import {SaveDialogConfig, SaveDialogResult} from "../models/SaveDialogConfig";
import {WebDialogService} from "./WebDialogService";

@injectable()
export class DialogService {

    public constructor(
        private readonly electronDialogService: ElectronDialogService) {
    }

    public async showMessageBox(config: MessageBoxConfig): Promise<MessageBoxButton> {
        console.log("Showing dialog box...");
        try {
            console.log("try WebModalService...");
            return await WebDialogService.showMessageBox(config);
        } catch (error) {
            console.error(error);
            try {
                console.log("try ElectronDialogService...");
                return await this.electronDialogService.showMessageBox(config);
            } catch (error) {
                console.error(error);
                return MessageBoxButton.None;
            }
        }
    }

    public async browseForSingleFile(title: string, filters: FilenameFilter[], defaultPath?: string): Promise<string | undefined> {
        const config = new OpenDialogConfig(title, defaultPath, filters, [ 'openFile' ]);
        let openResult: OpenDialogResult = await this.electronDialogService.showOpenDialog(config);
        if (openResult.canceled || openResult.filePaths.length === 0) {
            return undefined;
        }
        return openResult.filePaths[0];
    }

    public async promptForSave(title: string, filters: FilenameFilter[], defaultPath?: string): Promise<string | undefined> {
        const config = new SaveDialogConfig(title, defaultPath, filters, [ 'createDirectory', 'showOverwriteConfirmation' ]);
        let saveResult: SaveDialogResult = await this.electronDialogService.showSaveDialog(config);
        if (saveResult.canceled || !saveResult.filePath) {
            return undefined;
        }
        return saveResult.filePath;
    }

    public async promptForDir(title: string, defaultPath?: string): Promise<string | undefined> {
        const config = new OpenDialogConfig(title, defaultPath, [], [ 'openDirectory', 'createDirectory' ]);
        let openResult: OpenDialogResult = await this.electronDialogService.showOpenDialog(config);
        if (openResult.canceled || openResult.filePaths.length === 0) {
            return undefined;
        }
        return openResult.filePaths[0];
    }
}
