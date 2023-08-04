﻿import React, { useState } from "react";
import { Stack, Button, TextField } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import * as XLSX from 'xlsx';
import {
    MESSAGE_ERROR_FILE_TYPE,
    MESSAGE_ERROR_FILE_EMPTY,
    MESSAGE_ERROR_UPLOAD,
    MESSAGE_SUCCEED_UPLOAD,
    BTN_CHOOSE_FILE,
    BTN_CANCEL,
    BTN_UPLOAD_FILE,
    TABLE_UPLOAD_FILE,
    ALERT_UPLOAD_START
} from '../constants';
import { axiosDeliveryUpload } from '../axios/Axios';
import CargoContent from './CargoContent';
import UploadPreviewContent from './UploadPreviewContent';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";

const UploadDashboard = ({ cargoNos }) => {
    const [file, setFile] = useState(null)
    const [fileName, setFileName] = useState(null)
    const [fileData, setFileData] = useState(null)
    const [fileTypeError, setFileTypeError] = useState(null)
    const [uploadResult, setUploadResult] = useState(null)
    const [uploadDescriptionResult, setUploadResultDescription] = useState(null)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [resultAlertOpen, setReultAlertOpen] = useState(false)

    const handleFileSelect = (e) => {
        clearFile()
        clearResult()
        setLoadingAlertOpen(true)

        let fileTypes = ['application/vnd.ms-excel', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'text/csv']
        let selectedFile = e.target.files[0]
        if (selectedFile) {
            if (fileTypes.includes(selectedFile.type)) {
                let reader = new FileReader();
                reader.readAsArrayBuffer(selectedFile)
                reader.onload = (e) => {
                    setFile(e.target.result)
                    setFileName(selectedFile.name)
                    handlePreviewFileData(e.target.result)
                }
            } else {
                setFileTypeError(MESSAGE_ERROR_FILE_TYPE)
                setLoadingAlertOpen(false)
            }
        } else {
            setFileTypeError(MESSAGE_ERROR_FILE_EMPTY)
            setLoadingAlertOpen(false)
        }
    }

    const handlePreviewFileData = (file) => {
        if (file !== null) {
            const workbook = XLSX.read(file, { type: 'buffer' })
            const worksheetName = workbook.SheetNames[0]
            const worksheet = workbook.Sheets[worksheetName]
            const data = XLSX.utils.sheet_to_json(worksheet)
            setFileData(data)
            setLoadingAlertOpen(false)
        } else {
            setFileTypeError(MESSAGE_ERROR_FILE_EMPTY)
            setLoadingAlertOpen(false)
        }
    }

    const handleFileUpload = (e) => {
        // Show alert
        setConfirmAlertOpen(true)
    }

    const handleFileCancel = (e) => {
        clearFile()
    }

    const handleConfirmButtonClick = () => {
        // Close alert
        setConfirmAlertOpen(false)
        // Call post api
        requestPostUploadApi()
    }

    const handleCancelButtonClick = () => {
        setConfirmAlertOpen(false)
        setReultAlertOpen(false)
        clearFile()
        clearResult()
    }

    async function requestPostUploadApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify({
                FileName: fileName,
                FileData: fileData
            })
            const response = await axiosDeliveryUpload(json)
            if (response.data.result == true) {
                setUploadResult(true)
                setUploadResultDescription(MESSAGE_SUCCEED_UPLOAD)
                setReultAlertOpen(true)
            } else {
                setUploadResult(false)
                setUploadResultDescription(MESSAGE_ERROR_UPLOAD)
                setReultAlertOpen(true)
            }
        } catch (error) {
            setUploadResult(false)
            setUploadResultDescription(MESSAGE_ERROR_UPLOAD + ": " + error.response.data.error)
            setReultAlertOpen(true)
        }
        setLoadingAlertOpen(false)
    }

    const clearFile = () => {
        setFile(null)
        setFileName(null)
        setFileData(null)
        setFileTypeError(null)
    }

    const clearResult = () => {
        setUploadResult(null)
        setUploadResultDescription(null)
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                cargoNos={cargoNos}
                setSelectedCargoNo={null} />
        </div>
        <div className="col-sm-6 h-100">
            <UploadPreviewContent fileData={fileData} />
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={2} direction="column" className="h-100">
                <div className="card card-primary h-100">
                    <div className="card-header">{TABLE_UPLOAD_FILE}</div>
                    <div className="card-body">
                        <Stack spacing={1} direction="column">
                            <TextField
                                variant={fileName ? "outlined" : "filled"}
                                size="small"
                                label={fileName ? fileName : MESSAGE_ERROR_FILE_EMPTY}
                                color="success"
                                disabled />
                            <Button
                                variant="contained"
                                component="label"
                                startIcon={<UploadFileIcon />}
                                onChange={handleFileSelect} >
                                {BTN_CHOOSE_FILE}
                                <input type="file" hidden />
                            </Button>
                        </Stack>
                    </div>
                </div>

                <Stack spacing={2} direction="column">
                    <Button variant="contained" color="primary" size="large" onClick={handleFileUpload}>{BTN_UPLOAD_FILE}</Button>
                    <Button variant="contained" color="secondary" size="large" onClick={handleFileCancel}>{BTN_CANCEL}</Button>
                </Stack>
            </Stack>
        </div>
        <MuiConfirmDialog
            open={confirmAlertOpen}
            title={TABLE_UPLOAD_FILE}
            contentText={ALERT_UPLOAD_START}
            handlePrimaryButtonClick={handleConfirmButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={!fileTypeError ? "success" : "error"}
            open={fileTypeError ? true : false}
            title={TABLE_UPLOAD_FILE}
            contentText={fileTypeError}
            handleButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={uploadResult ? "success" : "error"}
            open={resultAlertOpen}
            title={TABLE_UPLOAD_FILE}
            contentText={uploadDescriptionResult}
            handleButtonClick={handleCancelButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default UploadDashboard