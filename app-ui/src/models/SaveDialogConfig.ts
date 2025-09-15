import {FilenameFilter} from "./FilenameFilter";

export class SaveDialogConfig {

    public constructor(
        public title?: string,
        public defaultPath?: string,
        public filters?: FilenameFilter[],
        public properties?: Array<'showHiddenFiles' | 'createDirectory' | 'treatPackageAsDirectory' | 'showOverwriteConfirmation' | 'dontAddToRecent'>) {
    }

}

export class SaveDialogResult {

    public constructor(
        public canceled: boolean,
        public filePath?: string) {
    }

}
