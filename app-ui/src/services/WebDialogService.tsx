import {MessageBoxButton, MessageBoxConfig} from "../models/MessageBoxConfig";
import React from 'react';
import ModalDialog from "../components/Modal/ModalDialog";
import {createRoot} from "react-dom/client";

interface WebDialogModalService {
    showMessageBox: (config: MessageBoxConfig) => Promise<MessageBoxButton>;
}

const modalHost = document.getElementById('modalHost') ?? document.body;

const createWebDialogModalService = (): WebDialogModalService => {

    const showMessageBox = (config: MessageBoxConfig) => {

        return new Promise<MessageBoxButton>( resolve => {

            let isOpen = true;
            const modalNode = document.createElement("div");
            modalHost.appendChild(modalNode);
            const root = createRoot(modalNode);

            const close: (button: MessageBoxButton) => void = (button) => {
                isOpen = false;
                resolve(button);
                root.unmount();
                modalHost.removeChild(modalNode);
            }

            const ModalWrapper: React.FC = () => {
                return <ModalDialog modalHost={modalNode} isOpen={isOpen} onClose={close} contentConfig={config} />;
            };

            root.render(<ModalWrapper/>);
        });

    }

    return { showMessageBox };
};

export const WebDialogService: WebDialogModalService = createWebDialogModalService();
