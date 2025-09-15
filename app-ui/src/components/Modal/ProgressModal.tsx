import {ApiCommsService} from "../../services/ApiCommsService";
import {useService} from "../../ContainerContext";
import {useEffect, useState} from "react";
import {Button} from "@mui/material";

const ProgressModal = () => {

    const [visible, setVisible] = useState<boolean>(false);
    const [taskId, setTaskId] = useState<string>('');
    const [title, setTitle] = useState<string>('');
    const [statusLine1, setStatusLine1] = useState<string>('');
    const [statusLine2, setStatusLine2] = useState<string>('');
    const [workTotal, setWorkTotal] = useState<number>(0);
    const [workCompleted, setWorkCompleted] = useState<number>(0);
    const [isCancelRequested, setIsCancelRequested] = useState<boolean>(false);

    const comms = useService(ApiCommsService);

    const handleProgress = (d: any) => {
        if (d) {
            if (!visible && d.showDialog === true) {
                setIsCancelRequested(false);
                setTaskId(d.taskId);
                setTitle(d.title);
                setVisible(true);
            } else if (d.showDialog === false) {
                setVisible(false);
                return;
            }
            setStatusLine1(d.statusLine1);
            setStatusLine2(d.statusLine2);
            setWorkTotal(d.total);
            setWorkCompleted(d.completed);
        }
    }

    const handleCancelRequest = () => {
        if (taskId && !isCancelRequested) {
            const action = async () => {
                await comms.cancelInteractiveTask(taskId);
            }
            setIsCancelRequested(true);
            action().then().catch(err => console.error(err));
        }
    }

    useEffect(() => {
        comms.OnTaskProgress.subscribe(handleProgress);
        return () => {
            comms.OnTaskProgress.unsubscribe(handleProgress);
            if (visible) setVisible(false);
        }
    }, []);

    if (!visible) {
        return null;
    }

    return (
        <div className="progress-modal-overlay">
            <div className="modal-content" onScroll={(e) => e.stopPropagation()} onClick={(e) => e.stopPropagation()}>
                <div className="dialog-outer-wrapper">
                    <div className="dialog-title-wrapper">
                        {title}
                    </div>
                    <div className="progress-dialog-content-wrapper">
                        {(() => {
                            if (workTotal < 0)
                                return <div></div>
                            return <progress value={workCompleted} max={workTotal}></progress>
                        })()}
                        {(() => {
                            if (workTotal < 0)
                                return <div><p>Processing...</p></div>
                            return <div><p>{workCompleted} of {workTotal}</p></div>
                        })()}
                        <div>
                            {statusLine1}
                        </div>
                        <div>
                            {statusLine2}
                        </div>
                    </div>
                    <div className="dialog-controls-wrapper">
                        <div className="dialog-controls">
                            <Button type="button" variant={'outlined'} disabled={isCancelRequested} onClick={() => handleCancelRequest()}>Cancel</Button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default ProgressModal;
