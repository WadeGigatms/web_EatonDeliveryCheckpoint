import React, { useState, useEffect } from 'react';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoDataInfoContent from './CargoDataInfoContent';
import MuiFormDialog from "./MuiFormDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";
import {
    TABLE_SEARCH,
    MESSAGE_SEARCH_DN,
    BTN_CONFIRM,
    BTN_CANCEL,
} from '../constants';
import { Stack, Button } from '@mui/material';
import {
    axiosDeliverySearchGetApi
} from '../axios/Axios';

const SearchDashboard = ({ deliveryCargoDtos }) => {
    const [searchFormAlertOpen, setSearchFormAlertOpen] = useState(false)
    const [inputErrorAlertOpen, setInputErrorAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [selectedDeliveryCargoDto, setSelectedDeliveryCargoDto] = useState(null)
    const [value, setValue] = useState("")

    useEffect(() => {
        setSearchFormAlertOpen(true)
    }, [])

    const handleTextFieldChange = (e) => {
        setValue(e.target.value)
    }

    const handlePrimaryButtonClick = (e) => {
        if (value === "") {
            setInputErrorAlertOpen(true)
        } else {
            setSearchFormAlertOpen(false)
            requestReviewGetApi()
        }
    }

    const handleSecondaryButtonClick = (e) => {

    }

    async function requestReviewGetApi() {
        setLoadingAlertOpen(true)
        try {
            const response = await axiosDeliverySearchGetApi(value)
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
                deliveryStep={0}
                deliveryCargoDtos={deliveryCargoDtos}
                selectedDeliveryCargoDto={null}
                setSelectedDeliveryCargoDto={null} />
        </div>
        <div className="col-sm-6 h-100">
            <CargoDataContent
                deliveryStep={3}
                selectedDeliveryCargoDto={selectedDeliveryCargoDto} />
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataInfoContent deliveryStep={3} selectedDeliveryCargoDto={selectedDeliveryCargoDto} className="h-100" />
                <Button variant="contained" color="primary" size="large" onClick={() => setSearchFormAlertOpen(true)}>{TABLE_SEARCH}</Button>
            </Stack>
        </div>
        <MuiFormDialog
            open={searchFormAlertOpen}
            onClose={() => setSearchFormAlertOpen(false)}
            value={value}
            onChange={handleTextFieldChange}
            title={TABLE_SEARCH}
            contentText={MESSAGE_SEARCH_DN}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handlePrimaryButtonClick}
            handleSecondaryButtonClick={handleSecondaryButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default SearchDashboard