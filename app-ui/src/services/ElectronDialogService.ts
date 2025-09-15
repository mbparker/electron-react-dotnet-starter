import {
    MessageBoxOptions,
    MessageBoxReturnValue,
    OpenDialogOptions,
    OpenDialogReturnValue,
    SaveDialogOptions, SaveDialogReturnValue
} from "electron";
import { ElectronApiService } from "./ElectronApiService";
import {MessageBoxButton, MessageBoxConfig, MessageBoxHelpers, MessageBoxType} from "../models/MessageBoxConfig";
import {OpenDialogConfig, OpenDialogResult} from "../models/OpenDialogConfig";
import {SaveDialogConfig, SaveDialogResult} from "../models/SaveDialogConfig";
import {injectable} from "tsyringe";

@injectable()
export class ElectronDialogService {

    public constructor(
        private readonly electronApi: ElectronApiService) {
    }

    public async showMessageBox(config: MessageBoxConfig): Promise<MessageBoxButton> {
        const electronOptions = this.convertMessageBoxConfigToElectronOptions(config);
        const dialogResult = await this.electronApi.showNativeMessageBox(electronOptions);
        return this.mapElectronResponseNumberToMessageBoxButton(config, dialogResult);
    }

    public async showOpenDialog(config: OpenDialogConfig): Promise<OpenDialogResult> {
        const options = this.convertOpenDialogConfigToElectronOptions(config);
        const openResponse = await this.electronApi.showNativeOpenDialog(options);
        return this.mapElectronOpenDialogReturnValueToOpenDialogResult(openResponse);
    }

    public async showSaveDialog(config: SaveDialogConfig): Promise<SaveDialogResult> {
        const options = this.convertSaveDialogConfigToElectronOptions(config);
        const openResponse = await this.electronApi.showNativeSaveDialog(options);
        return this.mapElectronSaveDialogReturnValueToSaveDialogResult(openResponse);
    }

    private convertMessageBoxConfigToElectronOptions(config: MessageBoxConfig): MessageBoxOptions {
        const buttonTexts = this.getElectronMessageBoxButtonTexts(config.buttons);
        return {
            message: config.message,
            type: this.getElectronMessageBoxType(config.type),
            title: config.title,
            buttons: buttonTexts,
            defaultId: this.getButtonIndex(buttonTexts, MessageBoxHelpers.getButtonText(config.defaultButton)),
            cancelId: this.getButtonIndex(buttonTexts, MessageBoxHelpers.getButtonText(config.cancelButton)),
            noLink: true
        };
    }

    private mapElectronResponseNumberToMessageBoxButton(config: MessageBoxConfig, returnValue: MessageBoxReturnValue): MessageBoxButton {
        const buttonTexts = this.getElectronMessageBoxButtonTexts(config.buttons);
        if (returnValue.response >= 0) {
            const clickedButtonText = buttonTexts[returnValue.response];
            return this.getMessageBoxButtonFromText(clickedButtonText);
        }
        return MessageBoxButton.None;
    }

    private getElectronMessageBoxType(type: MessageBoxType): 'none' | 'info' | 'error' | 'question' | 'warning' | undefined {
        switch(type) {
            case MessageBoxType.None:
                return 'none';
            case MessageBoxType.Info:
                return 'info';
            case MessageBoxType.Error:
                return 'error';
            case MessageBoxType.Question:
                return 'question';
            case MessageBoxType.Warning:
                return 'warning';
        }
        return undefined;
    }

    private getElectronMessageBoxButtonTexts(buttons: MessageBoxButton[]): string[] {
        let result = [];
        let texts =  buttons.map(x => MessageBoxHelpers.getButtonText(x));
        texts.reverse();
        for(let text of texts) {
            if (text) {
                result.push(text);
            }
        }
        return result;
    }

    private getMessageBoxButtonFromText(text: string): MessageBoxButton {
        switch(text) {
            case 'OK':
                return MessageBoxButton.Ok;
            case 'Yes':
                return MessageBoxButton.Yes;
            case 'No':
                return MessageBoxButton.No;
            case 'Cancel':
                return MessageBoxButton.Cancel;
            case 'Abort':
                return MessageBoxButton.Abort;
            case 'Retry':
                return MessageBoxButton.Retry;
        }
        return MessageBoxButton.None;
    }

    private getButtonIndex(buttons: string[], button?: string): number | undefined {
        if (button) {
            const index = buttons.indexOf(button);
            if (index >= 0) {
                return index;
            }
        }
        return undefined;
    }

    private convertOpenDialogConfigToElectronOptions(config: OpenDialogConfig): OpenDialogOptions {
        return {
            title: config.title, message: config.title, defaultPath: config.defaultPath,
            filters: config.filters, properties: config.properties
        };
    }

    private mapElectronOpenDialogReturnValueToOpenDialogResult(openResponse: OpenDialogReturnValue): OpenDialogResult {
        return new OpenDialogResult(openResponse.canceled, openResponse.filePaths);
    }

    private convertSaveDialogConfigToElectronOptions(config: SaveDialogConfig): SaveDialogOptions {
        return {
            title: config.title, message: config.title, defaultPath: config.defaultPath,
            filters: config.filters, properties: config.properties
        };
    }

    private mapElectronSaveDialogReturnValueToSaveDialogResult(saveResponse: SaveDialogReturnValue): SaveDialogResult {
        return new SaveDialogResult(saveResponse.canceled, saveResponse.filePath);
    }
}
