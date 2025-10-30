import * as signalR from "@microsoft/signalr"
import {EventEmitter} from "../utils/EventEmitter";
import {injectable} from "tsyringe";
import {Utils} from "../utils/Utils";
import {ElectronApiService} from "./ElectronApiService";

@injectable()
export class ApiCommsService {

    private hubConnection: signalR.HubConnection = <any>undefined;
    private connected: boolean = false;

    public constructor(
        private readonly electronApi: ElectronApiService) {
    }

    public OnReconnecting: EventEmitter<Error | undefined> = new EventEmitter<Error | undefined>();
    public OnPingClient: EventEmitter<any> = new EventEmitter<any>();
    public OnAppNotification: EventEmitter<any> = new EventEmitter<any>();
    public OnTaskProgress: EventEmitter<any> = new EventEmitter<any>();

    public get isConnected(): boolean {
        return this.connected;
    }

    public async startConnection(): Promise<void> {
        const apiUrl = await this.electronApi.getApiUrl();
        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${apiUrl}/comms`)
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.hubConnection.onreconnecting(err => this.OnReconnecting.emit(err));

        this.hubConnection.onreconnected((connectionId) => {
            this.connected = true;
            console.log(`Connection re-established: ${connectionId}`);
        });

        this.hubConnection.onclose(err => {
            this.connected = false;
            if (err) {
                console.error(err);
            }
        });

        this.hubConnection.on('PingClient', evt => this.OnPingClient.emit(evt));
        this.hubConnection.on('AppNotification', evt => this.OnAppNotification.emit(evt));
        this.hubConnection.on('TaskProgress', evt => this.OnTaskProgress.emit(evt));

        this.OnReconnecting.subscribe((evt) => {
            this.connected = false;
            console.log('Connection dropped. Reconnecting to API...');
            if (evt) {
                console.error(evt);
            }
        });

        this.OnPingClient.subscribe(evt => console.log('RECEIVED PING: ' + JSON.stringify(evt)));

        for(let i=1;i<=10;i++) {
            try {
                await this.hubConnection.start();
                this.connected = true;
                console.log(`Connection started after ${i} attempts. Connection ID: ${this.hubConnection.connectionId}`);
                return;
            }
            catch (err) {
                if (i == 10) {
                    console.error('FATAL: Failed to connect to API.');
                    throw err;
                }

                await Utils.sleep(500);
            }
        }

    }

    public async stopConnection(): Promise<void> {
        if (this.connected) {
            await this.hubConnection.stop();
        }
    }

    public async pingServer(data: any): Promise<any> {
        return await this.hubConnection.invoke('PingServer', data);
    }

    public async clientReady(): Promise<void> {
        await this.hubConnection.invoke('ClientReady');
    }

    public async cancelInteractiveTask(taskId: string): Promise<void> {
        await this.hubConnection.invoke('CancelInteractiveTask', taskId);
    }
}
