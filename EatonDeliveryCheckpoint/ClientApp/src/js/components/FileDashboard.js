import React, { useEffect, useState } from "react";
import { Stack, Button, TextField } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import * as XLSX from 'xlsx';
import CargoContent from './CargoContent';
import FilePreviewContent from './FilePreviewContent';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";
import {
    MESSAGE_ERROR_HEADER,
    MESSAGE_ERROR_FILE_TYPE,
    MESSAGE_ERROR_FILE_EMPTY,
    MESSAGE_ERROR_UPLOAD,
    MESSAGE_SUCCEED_UPLOAD,
    MESSAGE_FILE_UPLOAD,
    MESSAGE_DELETE_FILE,
    MESSAGE_SUCCEED_DELETE,
    MESSAGE_ERROR_DELETE,
    BTN_CHOOSE_FILE,
    BTN_CONFIRM,
    BTN_CANCEL,
    BTN_UPLOAD_FILE,
    BTN_DELETE_FILE,
    BTN_EDIT_FILE,
    TABLE_UPLOAD_FILE,
    TABLE_EDIT_FILE,
} from '../constants';
import { axiosDeliveryUploadPostApi, axiosDeliveryDisablePostApi } from '../axios/Axios';

const FileDashboard = ({ deliveryNumberDtos }) => {
    const [file, setFile] = useState(null)
    const [fileName, setFileName] = useState(null)
    const [fileData, setFileData] = useState(null)
    const [fileTypeError, setFileTypeError] = useState(null)
    const [fileTypeErrorAlertOpen, setFileTypeErrorAlertOpen] = useState(false)
    const [fileTypeErrorSeverity, setFileTypeErrorSeverity] = useState("success")
    const [uploadResult, setUploadResult] = useState(null)
    const [uploadDescriptionResult, setUploadResultDescription] = useState(null)
    const [uploadResultAlertOpen, setUploadResultAlertOpen] = useState(false)
    const [uploadResultSeverity, setUploadResultSeverity] = useState("success")
    const [deleteAlertOpen, setDeleteAlertOpen] = useState(false)
    const [deleteResult, setDeleteResult] = useState(null)
    const [deleteDescriptionResult, setDeleteResultDescription] = useState(null)
    const [deleteResultSeverity, setDeleteResultSeverity] = useState("success")
    const [deleteResultAlertOpen, setDeleteResultAlertOpen] = useState(false)
    const [deliveryStep, setDeliveryStep] = useState("new")
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)
    const [selectedDeliveryNumberDto, setSelectedDeliveryNumberDto] = useState(null)
    const [primaryButtonText, setPrimaryButtonText] = useState(BTN_UPLOAD_FILE)
    const [primaryButtonColor, setPrimaryButtonColor] = useState("primary")
    const [disablePrimaryButton, setDisablePrimaryButton] = useState(true)
    const [disableSecondaryButton, setDisableSecondaryButton] = useState(true)

    useEffect(() => {
        setFileTypeErrorSeverity(fileTypeError === null ? "error" : "error")
    }, [fileTypeError])

    useEffect(() => {
        setUploadResultSeverity(uploadResult === true ? "success" : "error")
    }, [uploadResult])

    useEffect(() => {
        setDeleteResultSeverity(deleteResult === true ? "success" : "error")
    }, [deleteResult])

    useEffect(() => {
        if (file) {
            // Did select file
            setPrimaryButtonText(BTN_UPLOAD_FILE)
            setPrimaryButtonColor("primary")
            setDisablePrimaryButton(false)
            setDisableSecondaryButton(false)
            return
        } else {
            if (deliveryNumberDtos && deliveryNumberDtos.length > 0) {
                setPrimaryButtonText(selectedDeliveryNumberDto ? BTN_DELETE_FILE : BTN_EDIT_FILE)
                setPrimaryButtonColor(selectedDeliveryNumberDto ? "error" : "warning")
                setDisablePrimaryButton(deliveryStep === "select" && !selectedDeliveryNumberDto ? true : false)
                setDisableSecondaryButton(selectedDeliveryNumberDto || deliveryStep === "select" ? false : true)
                return
            } else {
                setPrimaryButtonText(BTN_EDIT_FILE)
                setPrimaryButtonColor("warning")
                setDisablePrimaryButton(true)
                setDisableSecondaryButton(true)
                return
            }
        }
    }, [file, deliveryNumberDtos, selectedDeliveryNumberDto, deliveryStep])

    const handleFileSelect = (e) => {
        clearFile()
        clearUploadResult()
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
            try {
                const workbook = XLSX.read(file, { type: 'buffer' })
                const worksheetName = workbook.SheetNames[0]
                const worksheet = workbook.Sheets[worksheetName]
                worksheet.A1.w = "Delivery"
                worksheet.B1.w = "Item"
                worksheet.C1.w = "Material"
                worksheet.D1.w = "Quantity"
                worksheet.E1.w = "Unit"
                if (worksheet.A1 && worksheet.B1 && worksheet.C1 && worksheet.D1 && worksheet.E1) {
                    const data = XLSX.utils.sheet_to_json(worksheet)
                    setFileData(data)
                    setLoadingAlertOpen(false)
                } else {
                    setLoadingAlertOpen(false)
                    clearFile()
                    setFileTypeError(MESSAGE_ERROR_HEADER)
                    setFileTypeErrorAlertOpen(true)
                    return
                }
            } catch (error) {
                console.log(error)
                setLoadingAlertOpen(false)
                clearFile()
                setFileTypeError(MESSAGE_ERROR_HEADER)
                setFileTypeErrorAlertOpen(true)
            }
        } else {
            setLoadingAlertOpen(false)
            clearFile()
            setFileTypeError(MESSAGE_ERROR_FILE_EMPTY)
            setFileTypeErrorAlertOpen(true)
        }
    }

    const handleConfirmDeleteButtonClick = (e) => {
        requestDeleteApi()
        setDeliveryStep("new")
    }

    const handleConfirmButtonClick = () => {
        // Close alert
        setConfirmAlertOpen(false)
        // Call post api
        requestPostUploadApi()
    }

    const handlePrimaryButtonClick = (e) => {
        if (file) {
            setConfirmAlertOpen(true)
            return
        } else {
            if (selectedDeliveryNumberDto) {
                setDeleteAlertOpen(true)
                return
            }
            if (deliveryNumberDtos) {
                setDeliveryStep("select")
                return
            }
        }
    }

    const handleSecondaryButtonClick = (e) => {
        setLoadingAlertOpen(false)
        setConfirmAlertOpen(false)
        setFileTypeErrorAlertOpen(false)
        setUploadResultAlertOpen(false)
        setDeleteAlertOpen(false)
        setDeleteResultAlertOpen(false)

        clearFile()
        clearUploadResult()
        clearDeleteResult()
        setSelectedDeliveryNumberDto(null)
        setDeliveryStep("new")
    }

    const clearFile = () => {
        setFile(null)
        setFileName(null)
        setFileData(null)
        setFileTypeError(null)
    }

    const clearUploadResult = () => {
        setUploadResultDescription(null)
    }

    const clearDeleteResult = () => {
        setDeleteResultDescription(null)
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
                setUploadResultAlertOpen(true)
            } else {
                setUploadResult(false)
                setUploadResultDescription(MESSAGE_ERROR_UPLOAD)
                setUploadResultAlertOpen(true)
            }
        } catch (error) {
            setUploadResult(false)
            setUploadResultDescription(MESSAGE_ERROR_UPLOAD + ": " + error.response.data.error)
            setUploadResultAlertOpen(true)
        }
        setLoadingAlertOpen(false)
    }

    async function requestDeleteApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryNumberDto)
            const response = await axiosDeliveryDisablePostApi(json)
            if (response.data.result === true) {
                setSelectedDeliveryNumberDto(null)
                setDeleteResult(true)
                setDeleteResultDescription(MESSAGE_SUCCEED_DELETE)
                setDeleteResultAlertOpen(true)
            } else {
                setSelectedDeliveryNumberDto(null)
                setDeleteResult(false)
                setDeleteResultDescription(MESSAGE_ERROR_DELETE)
                setDeleteResultAlertOpen(true)
            }
        } catch (error) {
            setSelectedDeliveryNumberDto(null)
            setDeleteResult(false)
            setDeleteResultDescription(MESSAGE_ERROR_DELETE + ": " + error.response.data.error)
            setDeleteResultAlertOpen(true)
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
            <FilePreviewContent fileData={fileData} />
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
                    <Button variant="contained" color={primaryButtonColor} size="large" onClick={handlePrimaryButtonClick} disabled={disablePrimaryButton}>{primaryButtonText}</Button>
                    <Button variant="contained" color="secondary" size="large" onClick={handleSecondaryButtonClick} disabled={disableSecondaryButton}>{BTN_CANCEL}</Button>
                </Stack>
            </Stack>
        </div>
        <MuiConfirmDialog
            open={confirmAlertOpen}
            onClose={handleSecondaryButtonClick}
            title={TABLE_UPLOAD_FILE}
            contentText={MESSAGE_FILE_UPLOAD}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handleConfirmButtonClick}
            handleSecondaryButtonClick={handleSecondaryButtonClick} />
        <MuiConfirmDialog
            open={deleteAlertOpen}
            onClose={() => setDeleteAlertOpen(false)}
            title={TABLE_EDIT_FILE}
            contentText={MESSAGE_DELETE_FILE}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handleConfirmDeleteButtonClick}
            handleSecondaryButtonClick={handleSecondaryButtonClick} />
        <MuiAlertDialog
            severity={fileTypeErrorSeverity}
            open={fileTypeErrorAlertOpen}
            onClose={null}
            title={TABLE_UPLOAD_FILE}
            contentText={fileTypeError}
            handleButtonClick={handleSecondaryButtonClick} />
        <MuiAlertDialog
            severity={uploadResultSeverity}
            open={uploadResultAlertOpen}
            onClose={null}
            title={TABLE_UPLOAD_FILE}
            contentText={uploadDescriptionResult}
            handleButtonClick={handleSecondaryButtonClick} />
        <MuiAlertDialog
            severity={deleteResultSeverity}
            open={deleteResultAlertOpen}
            onClose={null}
            title={TABLE_EDIT_FILE}
            contentText={deleteDescriptionResult}
            handleButtonClick={handleSecondaryButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default FileDashboard