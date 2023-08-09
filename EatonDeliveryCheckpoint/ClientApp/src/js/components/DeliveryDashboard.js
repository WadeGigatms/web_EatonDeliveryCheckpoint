import React, { useState, useEffect } from 'react';
import { Stack, Button } from '@mui/material';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoDataInfoContent from './CargoDataInfoContent';
import {
    BTN_DELIVERY_READY,
    BTN_DELIVERY_START,
    BTN_DELIVERY_RESTART,
    BTN_DELIVERY_FINISH,
    BTN_DELIVERY_ALERT_DISMISS,
    BTN_CANCEL
} from '../constants';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";

const DeliveryDashboard = ({ deliveryState, setDeliveryState, cargoNos }) => {
    const [deliveryStep, setDeliveryStep] = useState(0) // 0: unchecked, 1: checked, 2: deliverying, 3: alert
    const [selectedCargoNo, setSelectedCargoNo] = useState(null)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)

    const handlePrimaryButtonClick = (e) => {
        if (deliveryStep === 1 && selectedCargoNo === null) {
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
                if (selectedCargoNo) {
                    setConfirmAlertOpen(true)
                }
                break;
            }
            case 2: {

                break;
            }
            case 3: {

                break;
            }
        }
    }

    const handleSecondaryButtonClick = (e) => {
        setDeliveryStep(0)
        setSelectedCargoNo(null)
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
    }

    const handleCancelButtonClick = () => {
        setConfirmAlertOpen(false)
        setSelectedCargoNo(null)
    }

    function isDisabled(deliveryStage) {
        if ((deliveryStage === 0 && cargoNos) || (deliveryStage === 1 && selectedCargoNo)) {
            return false
        }
        return true
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryStep={deliveryStep}
                cargoNos={cargoNos}
                setSelectedCargoNo={setSelectedCargoNo} />
        </div>
        <div className="col-sm-6 h-100">
            <CargoDataContent
                deliveryStep={deliveryStep}
                selectedCargoNo={selectedCargoNo} />
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataInfoContent deliveryStep={deliveryStep} cargoNo={selectedCargoNo} className="h-100" />
                {
                    deliveryStep === 1 && selectedCargoNo === null ?
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
    </div>
}

export default DeliveryDashboard