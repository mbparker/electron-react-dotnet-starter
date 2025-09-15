import {FilenameFilter} from "./FilenameFilter";

export class OpenDialogConfig {

    public constructor(
        public title?: string,
        public defaultPath?: string,
        public filters?: FilenameFilter[],
        public properties?: Array<'openFile' | 'openDirectory' | 'multiSelections' | 'showHiddenFiles' | 'createDirectory' | 'promptToCreate' | 'noResolveAliases' | 'treatPackageAsDirectory' | 'dontAddToRecent'>) {
    }

}

export class OpenDialogResult {

    public constructor(
        public canceled: boolean,
        public filePaths: string[]) {
    }

}
