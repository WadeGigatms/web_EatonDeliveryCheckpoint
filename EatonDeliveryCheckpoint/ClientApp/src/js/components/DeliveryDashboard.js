import React, { useEffect, useState } from 'react';
import { Stack, Button } from '@mui/material';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoDataInfoContent from './CargoDataInfoContent';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";
import {
    TITLE_DELIVERY,
    BTN_DELIVERY_READY,
    BTN_DELIVERY_START,
    BTN_DELIVERY_FINISH,
    BTN_DELIVERY_CONTINUE,
    MESSAGE_INVALID_QTY,
    MESSAGE_INVALID_PN,
    MESSAGE_INVALID_ALERT,
    MESSAGE_INVALID_ALERT_REMOVE,
    MESSAGE_PAUSE,
    MESSAGE_DELIVERY_FINISH,
    MESSAGE_DELIVERY_START,
    MESSAGE_DELIVERY_QUIT,
    BTN_CONFIRM,
    BTN_CANCEL,
    BTN_CONTINUE,
    BTN_QUIT,
    BTN_FINISH
} from '../constants';
import {
    axiosDeliveryStartPostApi,
    axiosDeliveryFinishPostApi,
    axiosDeliveryDismissAlertPostApi,
    axiosDeliveryQuitPostApi,
    axiosDeliverySearchGetApi
} from '../axios/Axios';

const DeliveryDashboard = ({ setDeliveryState, deliveryCargoDtos, requestGetApi }) => {
    const [deliveryStep, setDeliveryStep] = useState(0) // 0: unchecked, 1: checked, 2: deliverying, 3: finish and review, -1: alert or pause
    const [selectedDeliveryCargoDto, setSelectedDeliveryCargoDto] = useState(null)
    const [startAlertOpen, setStartAlertOpen] = useState(false)
    const [quitAlertOpen, setQuitAlertOpen] = useState(false)
    const [finishAlertOpen, setFinishAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [invalidMaterialAlertOpen, setInvalidMaterialAlertOpen] = useState(false)
    const [invalidQtyAlertOpen, setInvalidQtyAlertOpen] = useState(false)
    const [pauseAlertOpen, setPauseAlertOpen] = useState(false)
    const [pauseMessage, setPauseMessage] = useState("")

    useEffect(() => {
        if (deliveryCargoDtos) {
            const deliveryingCargoDto = deliveryCargoDtos.find((dto) => dto.state === 0)
            if (deliveryingCargoDto === null || deliveryingCargoDto === undefined) {
                return
            } else {
                if (deliveryStep === 0) {
                    setPauseMessage(deliveryingCargoDto.no + "出貨作業尚未完成! " + MESSAGE_PAUSE)
                    setPauseAlertOpen(true)
                } else if (deliveryStep === 2) {
                    setSelectedDeliveryCargoDto(deliveryingCargoDto)
                }
            }
        }
    }, [deliveryCargoDtos, deliveryStep])

    useEffect(() => {
        if (selectedDeliveryCargoDto) {
            const invalidMaterialDataDto = selectedDeliveryCargoDto.datas.find((data) => data.count === -1 && data.alert === 1)
            const invalidQtyDataDto = selectedDeliveryCargoDto.datas.find((data) => data.count < data.realtime_product_count && data.alert === 1)
            if (invalidMaterialDataDto && deliveryStep === 2) {
                setInvalidMaterialAlertOpen(true)
            } else if (invalidQtyDataDto && deliveryStep === 2) {
                setInvalidQtyAlertOpen(true)
            } else {
                
            }
        } else {
            setSelectedDeliveryCargoDto(null)
        }
    }, [selectedDeliveryCargoDto, deliveryStep])

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
                    setStartAlertOpen(true)
                }
                break;
            }
            case 2: {
                // Examine finish or alert
                if (didFinishDelivery(selectedDeliveryCargoDto)) {
                    setFinishAlertOpen(true)
                } else {
                    setQuitAlertOpen(true)
                }
                break;
            }
            case 3: {
                // Move to step 0
                setDeliveryStep(0)
                setDeliveryState(0)
                setSelectedDeliveryCargoDto(null)
                break;
            }
            default: {
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
            case -1: {
                return BTN_DELIVERY_CONTINUE;
            }
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
                return BTN_FINISH;
            }
            default: {
                return "";
            }
        }
    }

    const handleStartButtonClick = () => {
        setStartAlertOpen(false)
        setDeliveryStep(2)
        setDeliveryState(1)
        requestStartPostApi()
    }

    const handleCancelButtonClick = () => {
        setStartAlertOpen(false)
        setQuitAlertOpen(false)
        setFinishAlertOpen(false)
    }

    const handleQuitButtonClick = () => {
        setQuitAlertOpen(false)
        setDeliveryStep(3)
        requestQuitPostApi()
    }

    const handleFinishButtonClick = () => {
        setFinishAlertOpen(false)
        setDeliveryStep(3)
        requestFinishPostApi()
    }

    const handleDismissAlertClick = () => {
        setInvalidMaterialAlertOpen(false)
        setInvalidQtyAlertOpen(false)
        requestDismissAlertPostApi()
    }

    const handleQuitFromPauseButtonClick = () => {
        setPauseAlertOpen(false)
        setPauseMessage("")
        setDeliveryStep(3)
        requestQuitPostApi()
    }

    const handleContinueButtonClick = () => {
        setPauseAlertOpen(false)
        setDeliveryStep(2)
        setDeliveryState(1)
        if (deliveryCargoDtos) {
            const deliveryingCargoDto = deliveryCargoDtos.find((dto) => dto.state === 0)
            setSelectedDeliveryCargoDto(deliveryingCargoDto)
        }
    }

    function didFinishDelivery(selectedDeliveryCargoDto) {
        if (selectedDeliveryCargoDto) {
            var realtimeCount = 0
            for (var i = 0; i < selectedDeliveryCargoDto.datas.length; i++) {
                var data = selectedDeliveryCargoDto.datas[i]
                if (data.count > -1 && data.alert === 0) {
                    realtimeCount += data.realtime_product_count
                }
            }
            if (realtimeCount === selectedDeliveryCargoDto.product_quantity) {
                return true
            } else {
                return false
            }
        }
        return false
    }

    function isDisabled(deliveryStage) {
        if (deliveryStage === 0 && deliveryCargoDtos) {
            if (deliveryCargoDtos.length > 0) {
                return false
            }
        } else if (deliveryStage === 1 && selectedDeliveryCargoDto) {
            return false
        } else if (deliveryStage === 2) {
            return false
        } else if (deliveryStage === 3) {
            return false
        }
        return true
    }

    function handlePrimaryButtonColor(deliveryStep) {
        if (deliveryStep === -1) {
            return "warning"
        } else if (deliveryStep === 2 && didFinishDelivery(selectedDeliveryCargoDto) === true) {
            return "success"
        } else if (deliveryStep === 3) {
            return "success"
        }
        return "primary"
    }

    async function requestQuitPostApi() {
        setLoadingAlertOpen(true)
        try {
            if (deliveryCargoDtos) {
                const deliveryingCargoDto = deliveryCargoDtos.find((dto) => dto.state === 0)
                const json = JSON.stringify(deliveryingCargoDto)
                const response = await axiosDeliveryQuitPostApi(json)
                if (response.data.result === true) {
                    requestReviewGetApi(deliveryingCargoDto.no)
                    requestGetApi()
                }
            }
        } catch (error) {
            console.log("requestQuitPostApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    async function requestDismissAlertPostApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryCargoDto)
            const response = await axiosDeliveryDismissAlertPostApi(json)
            if (response.data.result === true) {
                requestGetApi()
            }
        } catch (error) {
            console.log("requestDismissAlertPostApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    async function requestStartPostApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryCargoDto)
            const response = await axiosDeliveryStartPostApi(json)
            if (response.data.result === true) {
                requestGetApi()
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
            const response = await axiosDeliveryFinishPostApi(json)
            if (response.data.result === true) {
                requestReviewGetApi()
            }
        } catch (error) {
            console.log("requestFinishPostApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    async function requestReviewGetApi() {
        setLoadingAlertOpen(true)
        try {
            const response = await axiosDeliverySearchGetApi(selectedDeliveryCargoDto.no)
            if (response.data.result === true) {
                const deliveryCargoDtos = response.data.deliveryCargoDtos
                if (deliveryCargoDtos != null) {
                    console.log(response.data.deliveryCargoDtos[0])
                    setSelectedDeliveryCargoDto(deliveryCargoDtos[0])
                } else {
                    setSelectedDeliveryCargoDto(null)
                }
            }
        } catch (error) {
            console.log("axiosDeliverySearchGetApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryStep={deliveryStep}
                deliveryCargoDtos={deliveryCargoDtos}
                selectedDeliveryCargoDto={selectedDeliveryCargoDto}
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
                        : <Button variant="contained" color={handlePrimaryButtonColor(deliveryStep)} size="large" onClick={handlePrimaryButtonClick} disabled={isDisabled(deliveryStep)}>{renderButton(deliveryStep)}</Button>
                }
            </Stack>
        </div>
        <MuiConfirmDialog
            open={startAlertOpen}
            onClose={handleCancelButtonClick}
            title={TITLE_DELIVERY}
            contentText={MESSAGE_DELIVERY_START}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handleStartButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiConfirmDialog
            open={quitAlertOpen}
            onClose={handleCancelButtonClick}
            title={TITLE_DELIVERY}
            contentText={MESSAGE_DELIVERY_QUIT}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handleQuitButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiConfirmDialog
            open={pauseAlertOpen}
            onClose={null}
            title={TITLE_DELIVERY}
            contentText={pauseMessage}
            primaryButton={BTN_CONTINUE}
            secondaryButton={BTN_QUIT}
            handlePrimaryButtonClick={handleContinueButtonClick}
            handleSecondaryButtonClick={handleQuitFromPauseButtonClick} />
        <MuiAlertDialog
            severity={"success"}
            open={finishAlertOpen}
            onClose={null}
            title={BTN_DELIVERY_FINISH}
            contentText={MESSAGE_DELIVERY_FINISH}
            handleButtonClick={handleFinishButtonClick} />
        <MuiAlertDialog
            severity={"error"}
            open={invalidMaterialAlertOpen}
            onClose={null}
            title={MESSAGE_INVALID_ALERT}
            contentText={MESSAGE_INVALID_PN + MESSAGE_INVALID_ALERT_REMOVE}
            handleButtonClick={handleDismissAlertClick} />
        <MuiAlertDialog
            severity={"error"}
            open={invalidQtyAlertOpen}
            onClose={null}
            title={MESSAGE_INVALID_ALERT}
            contentText={MESSAGE_INVALID_QTY + MESSAGE_INVALID_ALERT_REMOVE}
            handleButtonClick={handleDismissAlertClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default DeliveryDashboard