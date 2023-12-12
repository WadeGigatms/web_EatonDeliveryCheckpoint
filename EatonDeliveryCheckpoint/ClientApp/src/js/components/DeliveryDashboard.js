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

const DeliveryDashboard = ({ deliveryStep, setDeliveryStep, deliveryNumberDtos, requestGetApi }) => {
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

        // check data from database
        if (deliveryNumberDtos !== null && deliveryNumberDtos !== undefined) {

            // Find deliverying in database
            const deliveryingNumberDto = deliveryNumberDtos.find((dto) => dto.state === "delivery" || dto.state === "alert")

            if (deliveryingNumberDto !== null && deliveryingNumberDto !== undefined) {
                // There is a deliveryingNumberDto in database

                if (deliveryStep === "delivery" || deliveryStep === "alert") {
                    setSelectedDeliveryNumberDto(deliveryingNumberDto)
                } else if (deliveryStep === "new") {
                    setPauseMessage(deliveryingNumberDto.no + "出貨作業尚未完成! " + MESSAGE_PAUSE)
                    setPauseAlertOpen(true)
                }
            } else {
                // There is no deliveryinigNumberDto in database

                if (deliveryStep === "delivery" || deliveryStep === "alert") {

                    if (selectedDeliveryNumberDto !== null) {
                        // search deliveryNumberDtos by no
                        const matchDeliveryNumberDto = deliveryNumberDtos.find((dto) => dto.state === "new" && selectedDeliveryNumberDto.no)

                        if (matchDeliveryNumberDto != null) {
                            requestGetApi()
                        } else {
                            setDeliveryStep("new")
                            setSelectedDeliveryNumberDto(null)
                        }
                    }
                }
            }
        }
    }, [deliveryStep, deliveryNumberDtos, selectedDeliveryNumberDto])

    useEffect(() => {
        if (selectedDeliveryNumberDto) {
            if (selectedDeliveryNumberDto.state === "new" && deliveryStep === "select") {
                // Set valid datas and invalid datas in CargoDataContent
                setValidDatas(selectedDeliveryNumberDto.datas)
                setInvalidDatas(null)

            } else if (selectedDeliveryNumberDto.state === "delivery" && deliveryStep === "delivery") {
                // Examine alert
                const invalidMaterialDataDto = selectedDeliveryNumberDto.datas.find((data) => data.product_count === -1 && data.alert === 1 && data.delivery === "-")
                const invalidQtyDataDto = selectedDeliveryNumberDto.datas.find((data) => data.product_count < data.realtime_product_count && data.alert === 1)
                if (invalidMaterialDataDto && deliveryStep === "delivery") {
                    setInvalidMaterialAlertOpen(true)
                } else if (invalidQtyDataDto && deliveryStep === "delivery") {
                    setInvalidQtyAlertOpen(true)
                }

                // Set valid datas and invalid datas in CargoDataContent
                setValidDatas(selectedDeliveryNumberDto.datas)
                setInvalidDatas(null)
            } else if (selectedDeliveryNumberDto.state === "finish" && deliveryStep === "finish") {
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
        switch (deliveryStep) {
            case "new": {
                // new to select
                setDeliveryStep("select")
                break;
            }
            case "select": {
                // select to delivery
                if (selectedDeliveryNumberDto) {
                    setStartAlertOpen(true)
                }
                break;
            }
            case "delivery": {
                // Examine finish or alert
                if (didFinishDelivery(selectedDeliveryNumberDto)) {
                    setFinishAlertOpen(true)
                } else {
                    setQuitAlertOpen(true)
                }
                break;
            }
            case "finish": {
                // Move back to "new"
                setDeliveryStep("new")
                setSelectedDeliveryNumberDto(null)
                break;
            }
            default: {
                break;
            }
        }
    }

    const handleSecondaryButtonClick = (e) => {
        setDeliveryStep("new")
        setSelectedDeliveryNumberDto(null)
    }

    const renderButton = (step) => {
        switch (step) {
            case "alert": {
                return BTN_DELIVERY_CONTINUE;
            }
            case "new": {
                return BTN_DELIVERY_READY;
            }
            case "select": {
                return BTN_DELIVERY_START;
            }
            case "delivery": {
                return BTN_DELIVERY_FINISH;
            }
            case "finish": {
                return BTN_FINISH;
            }
            default: {
                return "";
            }
        }
    }

    const handleStartButtonClick = () => {
        setStartAlertOpen(false)
        setDeliveryStep("delivery")
        requestStartPostApi()
    }

    const handleCancelButtonClick = () => {
        setStartAlertOpen(false)
        setQuitAlertOpen(false)
        setFinishAlertOpen(false)
    }

    const handleQuitButtonClick = () => {
        setQuitAlertOpen(false)
        setDeliveryStep("finish")
        requestFinishPostApi()
    }

    const handleFinishButtonClick = () => {
        setFinishAlertOpen(false)
        setDeliveryStep("finish")
        requestFinishPostApi()
    }

    const handleDismissAlertClick = () => {
        setInvalidMaterialAlertOpen(false)
        setInvalidQtyAlertOpen(false)
        requestDismissAlertPostApi()
    }

    const handleContinueButtonClick = () => {
        setPauseAlertOpen(false)
        setDeliveryStep("delivery")
    }

    const handleQuitFromPauseButtonClick = () => {
        setPauseAlertOpen(false)
        setPauseMessage("")
        setDeliveryStep("finish")
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

    function isDisabled(step) {
        if (step === "new" && deliveryNumberDtos) {
            if (deliveryNumberDtos.length > 0) {
                return false
            }
        } else if (step === "select" && selectedDeliveryNumberDto) {
            return false
        } else if (step === "delivery") {
            return false
        } else if (step === "finish") {
            return false
        }
        return true
    }

    function handlePrimaryButtonColor(deliveryStep) {
        if (deliveryStep === "alert") {
            return "warning"
        } else if (deliveryStep === "delivery" && didFinishDelivery(selectedDeliveryNumberDto) === true) {
            return "success"
        } else if (deliveryStep === "finish") {
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
            setLoadingAlertOpen(false)
        } catch (error) {
            setLoadingAlertOpen(false)
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
                    className={deliveryStep === "finish" ? "h-75" : "h-100"}
                    title={TABLE_REALTIME_MATERIAL}
                    deliveryStep={deliveryStep}
                    datas={validDatas} />
                {
                    deliveryStep === "finish" ? (
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
                    deliveryStep === "select" && selectedDeliveryNumberDto === null ?
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