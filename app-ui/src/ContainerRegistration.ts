import {container} from "tsyringe";
import {AppInit} from "./AppInit";
import {ApiCommsService} from "./services/ApiCommsService";
import { DialogService } from "./services/DialogService";
import { ElectronDialogService } from "./services/ElectronDialogService";
import { ElectronApiService } from "./services/ElectronApiService";

export class ContainerRegistration {

    private static didRegister: boolean = false;

    public static RegisterDependencies(): boolean {
        if (!ContainerRegistration.didRegister) {
            console.log("Register app dependencies");

            container.registerSingleton(AppInit);
            container.registerSingleton(ApiCommsService);
            container.registerSingleton(DialogService);
            container.registerSingleton(ElectronDialogService);
            container.registerSingleton(ElectronApiService);

            ContainerRegistration.didRegister = true;
        }

        return ContainerRegistration.didRegister;
    }
}
