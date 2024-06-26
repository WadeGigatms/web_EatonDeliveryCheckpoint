﻿import React, { useState, useEffect } from 'react';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoDataInfoContent from './CargoDataInfoContent';
import MuiFormDialog from "./MuiFormDialog";
import MuiProgress from "./MuiProgress";
import {
    TABLE_REALTIME_MATERIAL,
    TABLE_SEARCH,
    MESSAGE_SEARCH_DN,
    BTN_CONFIRM,
    BTN_SEARCH,
} from '../constants';
import { Stack, Button } from '@mui/material';
import {
    axiosDeliverySearchGetApi
} from '../axios/Axios';

const SearchDashboard = ({ deliveryNumberDtos }) => {
    const [searchFormAlertOpen, setSearchFormAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [selectedDeliveryNumberDto, setSelectedDeliveryNumberDto] = useState(null)
    const [value, setValue] = useState("")
    const [helperText, setHelperText] = useState("")
    const [validDatas, setValidDatas] = useState(null)
    const [invalidDatas, setInvalidDatas] = useState(null)

    useEffect(() => {
        setSearchFormAlertOpen(true)
    }, [])

    useEffect(() => {
        if (selectedDeliveryNumberDto) {
            const validRows = selectedDeliveryNumberDto.datas.filter((data) => data.alert === 0)
            const invalidRows = selectedDeliveryNumberDto.datas.filter((data) => data.alert === 1)
            setValidDatas(validRows)
            setInvalidDatas(invalidRows)
        } else {
            setValidDatas(null)
            setInvalidDatas(null)
        }
    }, [selectedDeliveryNumberDto])

    const handleTextFieldChange = (e) => {
        if (e.target.value !== "") {
            setHelperText("")
        }
        setValue(e.target.value)
    }

    const handlePrimaryButtonClick = (e) => {
        if (value !== "" && helperText === "") {
            setValue("")
            setHelperText("")
            requestSearchGetApi()
        }
    }

    const handleCloseButtonClick = (e) => {
        setSearchFormAlertOpen(false)
        setValue("")
        setHelperText("")
    }

    async function requestSearchGetApi() {
        setLoadingAlertOpen(true)
        try {
            const response = await axiosDeliverySearchGetApi(value)
            if (response.data.result === true) {
                const deliveryNumberDtos = response.data.deliveryNumberDtos
                if (deliveryNumberDtos != null) {
                    setSearchFormAlertOpen(false)
                    setSelectedDeliveryNumberDto(deliveryNumberDtos[0])
                } else {
                    setSelectedDeliveryNumberDto(null)
                }
            }
        } catch (error) {
            console.log("axiosDeliverySearchGetApi error")
            console.log(error)
            setHelperText(error.response.data.error)
        }
        setLoadingAlertOpen(false)
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryStep={"new"}
                deliveryNumberDtos={deliveryNumberDtos}
                selectedDeliveryNumberDto={null}
                setSelectedDeliveryNumberDto={null} />
        </div>
        <div className="col-sm-6 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataContent
                    className={"h-75"}
                    title={TABLE_REALTIME_MATERIAL}
                    deliveryStep={"finish"}
                    datas={validDatas} />
                <CargoDataContent
                    className={"h-25"}
                    title={TABLE_REALTIME_MATERIAL}
                    deliveryStep={"finish"}
                    datas={invalidDatas} />
            </Stack>
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <CargoDataInfoContent
                    className="h-100"
                    deliveryStep={"finish"}
                    selectedDeliveryCargoDto={selectedDeliveryNumberDto} />
                <Button variant="contained" color="primary" size="large" onClick={() => setSearchFormAlertOpen(true)}>{BTN_SEARCH}</Button>
            </Stack>
        </div>
        <MuiFormDialog
            open={searchFormAlertOpen}
            onClose={handleCloseButtonClick}
            value={value}
            onChange={handleTextFieldChange}
            title={TABLE_SEARCH}
            contentText={MESSAGE_SEARCH_DN}
            helperText={helperText}
            primaryButton={BTN_CONFIRM}
            handlePrimaryButtonClick={handlePrimaryButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default SearchDashboard