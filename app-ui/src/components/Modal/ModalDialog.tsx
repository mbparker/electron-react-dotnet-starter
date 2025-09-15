import ReactDOM from "react-dom";
import {MessageBoxButton, MessageBoxConfig, MessageBoxHelpers, MessageBoxType} from "../../models/MessageBoxConfig";
import Lucide from "../Base/Lucide";
import {Button} from "@mui/material";

interface ModalDialogProps {
    modalHost: HTMLElement;
    isOpen: boolean;
    onClose: (button: MessageBoxButton) => void;
    contentConfig: MessageBoxConfig;
}

const ModalDialog: React.FC<ModalDialogProps> = ({ modalHost, isOpen, onClose, contentConfig }: ModalDialogProps) => {
    if (!isOpen) return null;

    document.onkeydown = (e) => {
        if (e.key === "Enter") {
            onClose(contentConfig.defaultButton);
        } else if (e.key === "Escape") {
            onClose(contentConfig.cancelButton);
        }
    }

    return ReactDOM.createPortal(
        <div className="modal-overlay">
            <div className="modal-content" onScroll={(e) => e.stopPropagation()} onClick={(e) => e.stopPropagation()}>
                <div className="dialog-outer-wrapper">
                    <div className="dialog-title-wrapper">
                        {contentConfig.title}
                    </div>
                    <div className="dialog-content-wrapper">
                        <div className="dialog-image-wrapper">
                            {(() => {
                                switch (contentConfig.type) {
                                    case MessageBoxType.Question:
                                        return <Lucide icon="MessageCircleQuestion" className="dialog-image-question" />
                                    case MessageBoxType.Warning:
                                        return <Lucide icon="MessageCircleWarning" className="dialog-image-warn" />
                                    case MessageBoxType.Error:
                                        return <Lucide icon="MessageCircleX" className="dialog-image-error" />
                                    case MessageBoxType.Info:
                                        return <Lucide icon="MessageCircle" className="dialog-image" />
                                    default:
                                        return null
                                }
                            })()}
                        </div>
                        <div>
                            {contentConfig.message}
                        </div>
                    </div>
                    <div className="dialog-controls-wrapper">
                        <div className="dialog-controls">
                            {contentConfig.buttons.map((button: MessageBoxButton, index: number) => (
                                <Button type="button" variant={button === contentConfig.defaultButton ? 'contained' : 'outlined'} onClick={() => onClose(button)}>{MessageBoxHelpers.getButtonText(button)}</Button>
                            ))}
                        </div>
                    </div>
                </div>
            </div>
        </div>, modalHost);
};

export default ModalDialog;
