import React, { useEffect, useState } from "react";
import { Stack, Button, TextField } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import * as XLSX from 'xlsx';
import CargoContent from './CargoContent';
import UploadPreviewContent from './UploadPreviewContent';
import MuiConfirmDialog from "./MuiConfirmDialog";
import MuiAlertDialog from "./MuiAlertDialog";
import MuiProgress from "./MuiProgress";
import {
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
import { axiosDeliveryUploadPostApi, axiosDeliveryDeleteApi } from '../axios/Axios';

const UploadDashboard = ({ deliveryNumberDtos }) => {
    const [file, setFile] = useState(null)
    const [fileName, setFileName] = useState(null)
    const [fileData, setFileData] = useState(null)
    const [fileTypeError, setFileTypeError] = useState(null)
    const [uploadResult, setUploadResult] = useState(null)
    const [uploadDescriptionResult, setUploadResultDescription] = useState(null)
    const [uploadResultAlertOpen, setUploadReultAlertOpen] = useState(false)
    const [loadingAlertOpen, setLoadingAlertOpen] = useState(false)
    const [confirmAlertOpen, setConfirmAlertOpen] = useState(false)
    const [fileTypeErrorAlertOpen, setFileTypeErrorAlertOpen] = useState(false)
    const [fileTypeErrorSeverity, setFileTypeErrorSeverity] = useState("success")
    const [uploadResultSeverity, setUploadResultSeverity] = useState("success")
    const [selectedDeliveryNumberDto, setSelectedDeliveryNumberDto] = useState(null)
    const [deleteState, setDeleteState] = useState(null)
    const [deliveryStep, setDeliveryStep] = useState()
    const [deleteAlertOpen, setDeleteAlertOpen] = useState(false)
    const [deleteResult, setDeleteResult] = useState(null)
    const [deleteDescriptionResult, setDeleteResultDescription] = useState(null)
    const [deleteResultSeverity, setDeleteResultSeverity] = useState("success")
    const [deleteResultAlertOpen, setDeleteResultAlertOpen] = useState(false)

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
        setDeleteState(selectedDeliveryNumberDto ? true : false)
    }, [selectedDeliveryNumberDto])

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
            worksheet.A1.w = "No"
            worksheet.B1.w = "Delivery"
            worksheet.C1.w = "Item"
            worksheet.D1.w = "Material"
            worksheet.E1.w = "Quantity"
            const data = XLSX.utils.sheet_to_json(worksheet)

            setFileData(data)
            setLoadingAlertOpen(false)
        } else {
            setFileTypeError(MESSAGE_ERROR_FILE_EMPTY)
            setLoadingAlertOpen(false)
        }
    }

    const handleFileEditClick = (e) => {
        if (selectedDeliveryNumberDto === null || selectedDeliveryNumberDto === undefined) {
            setDeliveryStep(4)
        } else {
            if (deleteState === true) {
                setDeleteAlertOpen(true)
            } else {

            }
        }
    }

    const handleFileUploadClick = (e) => {
        // Show alert
        setConfirmAlertOpen(true)
    }

    const handleFileCancelClick = (e) => {
        clearFile()
    }

    const handleConfirmDeleteButtonClick = (e) => {
        requestDeleteApi()
        setDeliveryStep(0)
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
        setDeleteAlertOpen(false)
        setFile(null)
        setFileName(null)
        setFileData(null)
        setDeleteAlertOpen(false)
        setSelectedDeliveryNumberDto(null)
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

    async function requestDeleteApi() {
        setLoadingAlertOpen(true)
        try {
            const json = JSON.stringify(selectedDeliveryNumberDto)
            const response = await axiosDeliveryDeleteApi(json)
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

    function renderEditButton(deleteState) {
        return deleteState === true ? BTN_DELETE_FILE : BTN_EDIT_FILE
    }

    function isEditButtonDisabled(deliveryNumberDtos) {
        return deliveryNumberDtos !== null ? false : true;
    }

    function isUploadButtonDisabled(file) {
        return file === null ? true : false
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
                    <Button variant="contained" color="warning" size="large" onClick={handleFileEditClick} disabled={isEditButtonDisabled(deliveryNumberDtos)}>{renderEditButton(deleteState)}</Button>
                    <Button variant="contained" color="primary" size="large" onClick={handleFileUploadClick} disabled={isUploadButtonDisabled(file)}>{BTN_UPLOAD_FILE}</Button>
                    <Button variant="contained" color="secondary" size="large" onClick={handleFileCancelClick} disabled={isUploadButtonDisabled(file)}>{BTN_CANCEL}</Button>
                </Stack>
            </Stack>
        </div>
        <MuiConfirmDialog
            open={confirmAlertOpen}
            onClose={handleCancelButtonClick}
            title={TABLE_UPLOAD_FILE}
            contentText={MESSAGE_FILE_UPLOAD}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handleConfirmButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiConfirmDialog
            open={deleteAlertOpen}
            onClose={() => setDeleteAlertOpen(false)}
            title={TABLE_EDIT_FILE}
            contentText={MESSAGE_DELETE_FILE}
            primaryButton={BTN_CONFIRM}
            secondaryButton={BTN_CANCEL}
            handlePrimaryButtonClick={handleConfirmDeleteButtonClick}
            handleSecondaryButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={fileTypeErrorSeverity}
            open={fileTypeErrorAlertOpen}
            onClose={null}
            title={TABLE_UPLOAD_FILE}
            contentText={fileTypeError}
            handleButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={uploadResultSeverity}
            open={uploadResultAlertOpen}
            onClose={null}
            title={TABLE_UPLOAD_FILE}
            contentText={uploadDescriptionResult}
            handleButtonClick={handleCancelButtonClick} />
        <MuiAlertDialog
            severity={deleteResultSeverity}
            open={deleteResultAlertOpen}
            onClose={null}
            title={TABLE_EDIT_FILE}
            contentText={deleteDescriptionResult}
            handleButtonClick={handleCancelButtonClick} />
        <MuiProgress open={loadingAlertOpen} />
    </div>
}

export default UploadDashboard