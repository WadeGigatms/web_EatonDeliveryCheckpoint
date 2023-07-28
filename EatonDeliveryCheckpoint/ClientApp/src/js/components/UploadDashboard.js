import React, { useState } from "react";
import { Stack, Button, Box, TextField } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import * as XLSX from 'xlsx';
import {
    ERROR_FILE_TYPE,
    ERROR_FILE_EMPTY,
    ERROR_UPLOAD,
    SUCCEED_UPLOAD,
    FORM_BTN_CHOOSE,
    FORM_BTN_PREVIEW,
    FORM_BTN_CANCEL,
    FORM_BTN_UPLOAD,

} from '../constants';
import { axiosDeliveryUpload } from '../axios/Axios';
import CargoContent from './CargoContent';
import UploadPreviewContent from './UploadPreviewContent';

const UploadDashboard = ({ cargoNos }) => {
    const [file, setFile] = useState(null)
    const [fileName, setFileName] = useState(null)
    const [fileData, setFileData] = useState(null)
    const [fileTypeError, setFileTypeError] = useState(null)
    const [uploadResult, setUploadResult] = useState(null)

    async function requestPost() {
        try {
            const json = JSON.stringify({
                FileName: fileName,
                FileData: fileData
            })
            //console.log(json)
            const response = await axiosDeliveryUpload(json)
            if (response.data.result == true) {
                setUploadResult(SUCCEED_UPLOAD)
            }
        } catch (error) {
            setUploadResult(ERROR_UPLOAD + ": " + error.response.data.error)
        }
    }

    const handleFile = (e) => {
        let fileTypes = ['application/vnd.ms-excel', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'text/csv']
        let selectedFile = e.target.files[0]
        if (selectedFile) {
            if (fileTypes.includes(selectedFile.type)) {
                setFileTypeError(null)
                let reader = new FileReader();
                reader.readAsArrayBuffer(selectedFile)
                reader.onload = (e) => {
                    setFile(e.target.result)
                    setFileName(selectedFile.name)
                }
            } else {
                setFileTypeError(ERROR_FILE_TYPE)
                setFile(null)
                setFileName(null)
                setFileData(null)
                setUploadResult(null)
            }
        } else {
            setFileTypeError(ERROR_FILE_EMPTY)
            setFile(null)
            setFileName(null)
            setFileData(null)
            setUploadResult(null)
        }
    }

    const handlePreview = (e) => {
        e.preventDefault()
        if (file !== null) {
            const workbook = XLSX.read(file, { type: 'buffer' })
            const worksheetName = workbook.SheetNames[0]
            const worksheet = workbook.Sheets[worksheetName]
            const data = XLSX.utils.sheet_to_json(worksheet)
            setFileData(data)
        } else {
            setFileTypeError(ERROR_FILE_EMPTY)
            setFile(null)
            setFileName(null)
            setFileData(null)
            setUploadResult(null)
        }
    }

    const handleFileSubmit = (e) => {
        e.preventDefault()
        requestPost() // call post api
    }

    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent cargoNos={cargoNos} />
        </div>
        <div className="col-sm-6 h-100">
            <UploadPreviewContent fileName={fileName} fileData={fileData} />
        </div>
        <div className="col-sm-3 h-100">
            <div className="card card-primary">
                <div className="card-header">
                    上傳檔案
                </div>
                <div className="card-body">
                    <Stack spacing={1} direction="column">
                        <TextField
                            variant={fileName ? "outlined" : "filled"}
                            label={fileName ? fileName : ERROR_FILE_EMPTY}
                            color={fileTypeError ? "success" : "error"}
                            error={fileTypeError ? true : false}
                            helperText={fileTypeError}
                            disabled />
                        <Button
                            variant="contained"
                            size="large"
                            startIcon={<UploadFileIcon />}
                            component="label"
                            onChange={handleFile} >
                            {FORM_BTN_CHOOSE}
                            <input type="file" hidden />
                        </Button>
                        <Button helperText="Please" variant="outlined" size="large" onClick={handlePreview}>{FORM_BTN_PREVIEW}</Button>
                        <Button variant="outlined" size="large" color="primary" onClick={handleFileSubmit}>{FORM_BTN_UPLOAD}</Button>
                        <Button variant="outlined" size="large">{FORM_BTN_CANCEL}</Button>
                    </Stack>
                    <div className="form-group">
                        {fileTypeError && (
                            <div className="alert alert-danger" role="alert">{fileTypeError}</div>
                        )}
                    </div>
                </div>
                <div className="card-footer">
                    <div className="form-group">
                        {uploadResult && (
                            <div className="alert alert-danger" role="alert">{uploadResult}</div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    </div>
    /*
    return <>
        {excelData ? (renderPreview()) : (<></>)}
        <div className="card card-primary">
            <div className="card-header">
                上傳檔案
            </div>
            <form>
                <div className="card-body">
                    <div className="form-group">
                        <div className="input-form">
                            <TextField type="file" id="outlined-basic" variant="outlined" onChange={handleFile} />
                        </div>
                    </div>
                    <div className="form-group">
                        {typeError && (
                            <div className="alert alert-danger" role="alert">{typeError}</div>
                        )}
                    </div>
                </div>
                <div className="card-footer">
                    <div className="form-group">
                        <Button type="button" onClick={handlePreview} >{FORM_BTN_PREVIEW}</Button>
                        <Button type="button" onClick={handleFileSubmit} >{FORM_BTN_UPLOAD}</Button>
                        <Button type="button">{FORM_BTN_CANCEL}</Button>
                    </div>
                    <div className="form-group">
                        {uploadResult && (
                            <div className="alert alert-danger" role="alert">{uploadResult}</div>
                        )}
                    </div>
                </div>
            </form>
        </div>
    </>
    
    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            DidUploadFile
        </div>
        <div className="col-sm-6 h-100">
            {excelData ? (renderPreview()) : (<></>)}
        </div>
        <div className="col-sm-3 h-100">
            <div className="card card-primary">
                <div className="card-header">上傳檔案</div>
                <form>
                    <div className="card-body">
                        <div className="form-group">
                            <div className="input-form">
                                <TextField type="file" id="outlined-basic" variant="outlined" onChange={handleFile} />
                            </div>
                        </div>
                        <div className="form-group">
                            {typeError && (
                                <div className="alert alert-danger" role="alert">{typeError}</div>
                            )}
                        </div>
                    </div>
                    <div className="card-footer">
                        <div className="form-group">
                            <Button type="button" onClick={handlePreview} >{FORM_BTN_PREVIEW}</Button>
                            <Button type="button" onClick={handleFileSubmit} >{FORM_BTN_UPLOAD}</Button>
                            <Button type="button">{FORM_BTN_CANCEL}</Button>
                        </div>
                        <div className="form-group">
                            {uploadResult && (
                                <div className="alert alert-danger" role="alert">{uploadResult}</div>
                            )}
                        </div>
                    </div>
                </form>
            </div>
        </div>
    </div>
    */
}

export default UploadDashboard