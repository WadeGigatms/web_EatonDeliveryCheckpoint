import React, { useEffect, useState } from 'react';
import { Stack, Button } from '@mui/material';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoDataInfoContent from './CargoDataInfoContent';
import {
    BTN_DELIVERY_READY,
    BTN_DELIVERY_START,
    BTN_DELIVERY_RESTART,
    BTN_DELIVERY_FINISH,
    BTN_DELIVERY_QUIT,
    BTN_DELIVERY_ALERT_DISMISS,
    BTN_CANCEL
} from '../constants';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";
import { axiosDeliveryCargoStartPostApi, axiosDeliveryCargoFinishPostApi } from '../axios/Axios';

const DeliveryDashboard = ({ deliveryState, setDeliveryState, deliveryCargoDtos, requestGetApi }) => {
    const [deliveryStep, setDeliveryStep] = useState(0) // 0: unchecked, 1: checked, 2: deliverying, 3: alert
    const [selectedDeliveryCargoDto, setSelectedDeliveryCargoDto] = useState(null)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)
    const [quitAlertOpen, setQuitAlertOpen] = useState(false)
    const [finishAlertOpen, setFinishAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [invalidPalletAlertOpen, setInvalidPalletAlertOpen] = useState(false)

    useEffect(() => {
        if (deliveryCargoDtos) {
            const deliveryingCargoDto = deliveryCargoDtos.find((dto) => dto.state === 0)
            if (deliveryingCargoDto === null || deliveryingCargoDto === undefined) { return }
            setSelectedDeliveryCargoDto(deliveryingCargoDto)
            console.log(deliveryingCargoDto)
        }
    }, [deliveryCargoDtos])

    const handlePrimaryButtonClick = (e) => {
        if (deliveryStep === 1 && selectedDeliveryCargoDto === null) {
            setDeliveryStep(0)
            return
        }

        switch (deliveryStep) {
            case 0: {
                // Stage 0 to 1
                setDeliveryStep(1)
                break;
            }
            case 1: {
                // Stage 1 to 2
                if (selectedDeliveryCargoDto) {
                    setConfirmAlertOpen(true)
                }
                break;
            }
            case 2: {
                var count = 0
                for (var i = 0; i < selectedDeliveryCargoDto.datas.length; i++) {
                    count += selectedDeliveryCargoDto.datas[i]
                }
                if (count === selectedDeliveryCargoDto.product_quantity) {
                    setFinishAlertOpen(true)
                } else {
                    setQuitAlertOpen(true)
                }
                break;
            }
            case 3: {

                break;
            }
        }
    }

    const handleSecondaryButtonClick = (e) => {
        setDeliveryStep(0)
        setSelectedDeliveryCargoDto(null)
    }

    const renderButton = (deliveryStage) => {
        switch (deliveryStage) {
            case 0: {
                return BTN_DELIVERY_READY;
            }
            case 1: {
                return BTN_DELIVERY_START;
            }
            case 2: {
                return BTN_DELIVERY_FINISH;
            }
            case 3: {
                return BTN_DELIVERY_RESTART;
            }
            default: {
                return "";
            }
        }
    }

    const handleConfirmButtonClick = () => {
        setConfirmAlertOpen(false)
        setDeliveryStep(2)
        setDeliveryState(1)
        requestStartPostApi()
    }

    const handleCancelButtonClick = () => {
        setConfirmAlertOpen(false)
        setQuitAlertOpen(false)
        setFinishAlertOpen(false)
    }

    const handleQuitButtonClick = () => {
        setQuitAlertOpen(false)
        setDeliveryStep(3)
    }

    const handleFinishButtonClick = () => {
        setFinishAlertOpen(false)
        setDeliveryStep(3)
        requestFinishPostApi()
    }

    function isDisabled(deliveryStage) {
        if ((deliveryStage === 0 && deliveryCargoDtos !== null) ||
            (deliveryStage === 1 && selectedDeliveryCargoDto !== null) ||
            (deliveryStage === 2)) {
            return false
        }
        return true
    }

    async function requestStartPostApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryCargoDto)
            const response = await axiosDeliveryCargoStartPostApi(json)
            if (response.data.result === true) {
                requestGetApi()
                console.log("requestStartPostApi OK")
            }
        } catch (error) {
            console.log("requestStartPostApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    async function requestFinishPostApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryCargoDto)
            const response = await axiosDeliveryCargoFinishPostApi(json)
            if (response.data.result === true) {
                requestGetApi()
                console.log("requestFinishPostApi OK")
            }
        } catch (error) {
            console.log("requestFinishPostApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryStep={deliveryStep}
                deliveryCargoDtos={deliveryCargoDtos}
                setSelectedDeliveryCargoDto={setSelectedDeliveryCargoDto} />
        </div>
        <div className="col-sm-6 h-100">
            <CargoDataContent
                deliveryStep={deliveryStep}
                selectedDeliveryCargoDto={selectedDeliveryCargoDto} />
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataInfoContent deliveryStep={deliveryStep} selectedDeliveryCargoDto={selectedDeliveryCargoDto} className="h-100" />
                {
                    deliveryStep === 1 && selectedDeliveryCargoDto === null ?
                        <Button variant="contained" color="secondary" size="large" onClick={handleSecondaryButtonClick}>{BTN_CANCEL}</Button>
                        : <Button variant="contained" color="primary" size="large" onClick={handlePrimaryButtonClick} disabled={isDisabled(deliveryStep)}>{renderButton(deliveryStep)}</Button>
                }
            </Stack>
        </div>
        <MuiConfirmDialog
            open={confirmAlertOpen}
            title={BTN_DELIVERY_START}
            contentText={BTN_DELIVERY_START}
            handlePrimaryButtonClick={handleConfirmButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiConfirmDialog
            open={quitAlertOpen}
            title={BTN_DELIVERY_QUIT}
            contentText={BTN_DELIVERY_QUIT}
            handlePrimaryButtonClick={handleQuitButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={"success"}
            open={finishAlertOpen}
            title={BTN_DELIVERY_FINISH}
            contentText={BTN_DELIVERY_FINISH}
            handleButtonClick={handleFinishButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default DeliveryDashboard