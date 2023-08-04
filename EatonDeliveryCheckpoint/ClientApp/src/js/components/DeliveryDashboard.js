import React, { useState, useEffect } from 'react';
import { Stack, Button } from '@mui/material';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoRealtimeContent from './CargoRealtimeContent';
import {
    BTN_DELIVERY_READY,
    BTN_DELIVERY_START,
    BTN_DELIVERY_RESTART,
    BTN_DELIVERY_ALERT_DISMISS,
    BTN_CANCEL
} from '../constants';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";

const DeliveryDashboard = ({ cargoNos }) => {
    const [deliveryStage, setDeliveryStage] = useState(0) // 0: unchecked, 1: checked, 2: deliverying, 3: alert
    const [selectedCargoNo, setSelectedCargoNo] = useState(null)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)
    const [uncheckedAlertOpen, setUncheckedAlertOpen] = useState(false)

    const handleButtonClick = (e) => {
        switch (deliveryStage) {
            case 0: {
                // Stage 0 to 1
                setDeliveryStage(1)
                break;
            }
            case 1: {
                // Stage 1 to 2
                if (selectedCargoNo) {
                    setConfirmAlertOpen(true)
                } else {
                    setUncheckedAlertOpen(true)
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
        setDeliveryStage(2)
    }

    const handleCancelButtonClick = () => {
        setEditState(false)
        setSelectedCargoNo(null)
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryStage={deliveryStage}
                cargoNos={cargoNos}
                setSelectedCargoNo={setSelectedCargoNo} />
        </div>
        <div className="col-sm-6 h-100">
            <CargoDataContent selectedCargoNo={selectedCargoNo} />
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoRealtimeContent cargoNo={selectedCargoNo} className="h-100" />
                <Button variant="contained" color="primary" size="large" onClick={handleButtonClick} disabled={deliveryStage === 1 && !selectedCargoNo}>{renderButton(deliveryStage)}</Button>
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