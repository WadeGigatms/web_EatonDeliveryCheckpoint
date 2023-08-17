import React, { useEffect, useState } from "react";
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
} from '../constants';
import { axiosDeliveryUploadPostApi } from '../axios/Axios';
import CargoContent from './CargoContent';
import UploadPreviewContent from './UploadPreviewContent';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";

const UploadDashboard = ({ deliveryCargoDtos }) => {
    const [file, setFile] = useState(null)
    const [fileName, setFileName] = useState(null)
    const [fileData, setFileData] = useState(null)
    const [fileTypeError, setFileTypeError] = useState(null)
    const [uploadResult, setUploadResult] = useState(null)
    const [uploadDescriptionResult, setUploadResultDescription] = useState(null)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)
    const [uploadResultAlertOpen, setUploadReultAlertOpen] = useState(false)
    const [fileTypeErrorAlertOpen, setFileTypeErrorAlertOpen] = useState(false)
    const [fileTypeErrorSeverity, setFileTypeErrorSeverity] = useState("success")
    const [uploadResultSeverity, setUploadResultSeverity] = useState("success")


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
                    setLoadingAlertOpen(false)
                    setFile(e.target.result)
                    setFileName(selectedFile.name)
                    handlePreviewFileData(e.target.result)
                }
            } else {
                setLoadingAlertOpen(false)
                setFileTypeError(MESSAGE_ERROR_FILE_TYPE)
                setFileTypeErrorAlertOpen(true)
            }
        } else {
            setLoadingAlertOpen(false)
            setFileTypeError(MESSAGE_ERROR_FILE_EMPTY)
            setFileTypeErrorAlertOpen(true)
        }
    }

    const handlePreviewFileData = (file) => {
        setLoadingAlertOpen(true)
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
        setFileTypeErrorAlertOpen(false)
        setConfirmAlertOpen(false)
        setUploadReultAlertOpen(false)
        setFile(null)
        setFileName(null)
        setFileData(null)
    }

    async function requestPostUploadApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify({
                FileName: fileName,
                FileData: fileData
            })
            const response = await axiosDeliveryUploadPostApi(json)
            if (response.data.result === true) {
                setUploadResult(true)
                setUploadResultDescription(MESSAGE_SUCCEED_UPLOAD)
                setUploadReultAlertOpen(true)
            } else {
                setUploadResult(false)
                setUploadResultDescription(MESSAGE_ERROR_UPLOAD)
                setUploadReultAlertOpen(true)
            }
        } catch (error) {
            setUploadResult(false)
            setUploadResultDescription(MESSAGE_ERROR_UPLOAD + ": " + error.response.data.error)
            setUploadReultAlertOpen(true)
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

    function isDisabled(file) {
        return file === null ? true : false
    }

    useEffect(() => {
        setFileTypeErrorSeverity(fileTypeError === null ? "error" : "error")
    }, [fileTypeError])

    useEffect(() => {
        setUploadResultSeverity(uploadResult === true ? "success" : "error")
    }, [uploadResult])

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent
                deliveryCargoDtos={deliveryCargoDtos}
                setSelectedDeliveryCargoDtos={null} />
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
                    <Button variant="contained" color="primary" size="large" onClick={handleFileUpload} disabled={isDisabled(file)}>{BTN_UPLOAD_FILE}</Button>
                    <Button variant="contained" color="secondary" size="large" onClick={handleFileCancel} disabled={isDisabled(file)}>{BTN_CANCEL}</Button>
                </Stack>
            </Stack>
        </div>
        <MuiConfirmDialog
            open={confirmAlertOpen}
            title={TABLE_UPLOAD_FILE}
            contentText={TABLE_UPLOAD_FILE}
            handlePrimaryButtonClick={handleConfirmButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={fileTypeErrorSeverity}
            open={fileTypeErrorAlertOpen}
            title={TABLE_UPLOAD_FILE}
            contentText={fileTypeError}
            handleButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={uploadResultSeverity}
            open={uploadResultAlertOpen}
            title={TABLE_UPLOAD_FILE}
            contentText={uploadDescriptionResult}
            handleButtonClick={handleCancelButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default UploadDashboard