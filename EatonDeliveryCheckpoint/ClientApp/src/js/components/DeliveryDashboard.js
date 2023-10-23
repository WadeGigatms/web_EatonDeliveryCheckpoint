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
    BTN_FINISH,
    TABLE_REALTIME_MATERIAL,
} from '../constants';
import {
    axiosDeliveryStartPostApi,
    axiosDeliveryFinishPostApi,
    axiosDeliveryDismissAlertPostApi,
    axiosDeliveryReviewGetApi
} from '../axios/Axios';

const DeliveryDashboard = ({ setDeliveryState, deliveryNumberDtos, requestGetApi }) => {
    const [deliveryStep, setDeliveryStep] = useState(0) // 0: new, 1: select, 2: deliverying, 3: finish/search/review, 4: edit, -1: alert/pause
    const [selectedDeliveryNumberDto, setSelectedDeliveryNumberDto] = useState(null)
    const [startAlertOpen, setStartAlertOpen] = useState(false)
    const [quitAlertOpen, setQuitAlertOpen] = useState(false)
    const [finishAlertOpen, setFinishAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [invalidMaterialAlertOpen, setInvalidMaterialAlertOpen] = useState(false)
    const [invalidQtyAlertOpen, setInvalidQtyAlertOpen] = useState(false)
    const [pauseAlertOpen, setPauseAlertOpen] = useState(false)
    const [pauseMessage, setPauseMessage] = useState("")
    const [validDatas, setValidDatas] = useState(null)
    const [invalidDatas, setInvalidDatas] = useState(null)

    useEffect(() => {
        if (deliveryNumberDtos) {
            const deliveryingNumberDto = deliveryNumberDtos.find((dto) => dto.state === 0)
            if (deliveryingNumberDto === null || deliveryingNumberDto === undefined) {
                return
            } else {
                if (deliveryStep === 0) {
                    setPauseMessage(deliveryingNumberDto.no + "出貨作業尚未完成! " + MESSAGE_PAUSE)
                    setPauseAlertOpen(true)
                } else if (deliveryStep === 2) {
                    setSelectedDeliveryNumberDto(deliveryingNumberDto)
                }
            }
        }
    }, [deliveryNumberDtos, deliveryStep])

    useEffect(() => {
        if (selectedDeliveryNumberDto) {
            // Examine alert
            const invalidMaterialDataDto = selectedDeliveryNumberDto.datas.find((data) => data.product_count === -1 && data.alert === 1 && data.delivery === "-")
            const invalidQtyDataDto = selectedDeliveryNumberDto.datas.find((data) => data.product_count < data.realtime_product_count && data.alert === 1)
            if (invalidMaterialDataDto && deliveryStep === 2) {
                setInvalidMaterialAlertOpen(true)
            } else if (invalidQtyDataDto && deliveryStep === 2) {
                setInvalidQtyAlertOpen(true)
            }

            // Set valid datas and invalid datas
            if (deliveryStep === 1 || deliveryStep === 2) {
                setValidDatas(selectedDeliveryNumberDto.datas)
                setInvalidDatas(null)
            } else if (deliveryStep === 3) {
                const validDatas = selectedDeliveryNumberDto.datas.filter((data) => data.product_count > -1 && data.delivery !== "-" && data.alert === 0)
                const invalidDatas = selectedDeliveryNumberDto.datas.filter((data) => data.product_count === -1 || data.delivery === "-" || data.alert === 1)
                setValidDatas(validDatas)
                setInvalidDatas(invalidDatas)
            }
        } else {
            setValidDatas(null)
            setInvalidDatas(null)
        }
    }, [selectedDeliveryNumberDto, deliveryStep])

    const handlePrimaryButtonClick = (e) => {
        if (deliveryStep === 1 && selectedDeliveryNumberDto === null) {
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
                if (selectedDeliveryNumberDto) {
                    setStartAlertOpen(true)
                }
                break;
            }
            case 2: {
                // Examine finish or alert
                if (didFinishDelivery(selectedDeliveryNumberDto)) {
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
                setSelectedDeliveryNumberDto(null)
                break;
            }
            default: {
                break;
            }
        }
    }

    const handleSecondaryButtonClick = (e) => {
        setDeliveryStep(0)
        setSelectedDeliveryNumberDto(null)
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
        requestFinishPostApi()
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

    const handleContinueButtonClick = () => {
        setPauseAlertOpen(false)
        setDeliveryStep(2)
        setDeliveryState(1)
        if (deliveryNumberDtos) {
            const deliveryingNumberDto = deliveryNumberDtos.find((dto) => dto.state === 0)
            setSelectedDeliveryNumberDto(deliveryingNumberDto)
        }
    }

    const handleQuitFromPauseButtonClick = () => {
        setPauseAlertOpen(false)
        setPauseMessage("")
        setDeliveryStep(3)
        requestFinishPostApi()
    }

    function didFinishDelivery(selectedDeliveryCargoDto) {
        if (selectedDeliveryCargoDto) {
            var realtimeCount = 0
            for (var i = 0; i < selectedDeliveryCargoDto.datas.length; i++) {
                var data = selectedDeliveryCargoDto.datas[i]
                if (data.product_count > -1 && data.alert === 0) {
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
        if (deliveryStage === 0 && deliveryNumberDtos) {
            if (deliveryNumberDtos.length > 0) {
                return false
            }
        } else if (deliveryStage === 1 && selectedDeliveryNumberDto) {
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
        } else if (deliveryStep === 2 && didFinishDelivery(selectedDeliveryNumberDto) === true) {
            return "success"
        } else if (deliveryStep === 3) {
            return "success"
        }
        return "primary"
    }

    async function requestDismissAlertPostApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryNumberDto)
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
            const json = JSON.stringify(selectedDeliveryNumberDto)
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
            const json = JSON.stringify(selectedDeliveryNumberDto)
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
            const response = await axiosDeliveryReviewGetApi(selectedDeliveryNumberDto.no)
            if (response.data.result === true) {
                const deliveryNumberDtos = response.data.deliveryNumberDtos
                if (deliveryNumberDtos != null) {
                    setSelectedDeliveryNumberDto(deliveryNumberDtos[0])
                } else {
                    setSelectedDeliveryNumberDto(null)
                }
            }
        } catch (error) {
            console.log("requestReviewGetApi error")
            console.log(error)
        }
        setLoadingAlertOpen(false)
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryStep={deliveryStep}
                deliveryNumberDtos={deliveryNumberDtos}
                selectedDeliveryNumberDto={selectedDeliveryNumberDto}
                setSelectedDeliveryNumberDto={setSelectedDeliveryNumberDto} />
        </div>
        <div className="col-sm-6 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataContent
                    className={deliveryStep === 3 ? "h-75" : "h-100"}
                    title={TABLE_REALTIME_MATERIAL}
                    deliveryStep={deliveryStep}
                    datas={validDatas} />
                {
                    deliveryStep === 3 ? (
                        <CargoDataContent
                            className={"h-25"}
                            title={TABLE_REALTIME_MATERIAL}
                            deliveryStep={deliveryStep}
                            datas={invalidDatas} />
                        ) : (<></>)
                }
            </Stack>
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataInfoContent deliveryStep={deliveryStep} selectedDeliveryCargoDto={selectedDeliveryNumberDto} className="h-100" />
                {
                    deliveryStep === 1 && selectedDeliveryNumberDto === null ?
                        <Button
                            variant="contained"
                            color="secondary"
                            size="large"
                            onClick={handleSecondaryButtonClick}>{BTN_CANCEL}</Button>
                        : <Button
                            variant="contained"
                            color={handlePrimaryButtonColor(deliveryStep)}
                            size="large"
                            onClick={handlePrimaryButtonClick}
                            disabled={isDisabled(deliveryStep)}>{renderButton(deliveryStep)}</Button>
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