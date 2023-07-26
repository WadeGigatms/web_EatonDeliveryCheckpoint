import React, { useState } from "react";
import * as XLSX from 'xlsx';
import {
    ERROR_FILE_TYPE,
    ERROR_FILE_EMPTY,
    ERROR_UPLOAD,
    SUCCEED_UPLOAD,
    FORM_BTN_PREVIEW,
    FORM_BTN_CANCEL,
    FORM_BTN_UPLOAD,

} from '../constants';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import { axiosDeliveryUpload } from '../axios/Axios';

const UploadDashboard = () => {
    const [excelFile, setExcelFile] = useState(null)
    const [excelFileName, setExcelFileName] = useState(null)
    const [excelData, setExcelData] = useState(null)
    const [typeError, setTypeError] = useState(null)
    const [uploadResult, setUploadResult] = useState(null)

    async function requestPost() {
        try {
            const json = JSON.stringify({
                FileName: excelFileName,
                FileData: excelData
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
                setTypeError(null)
                let reader = new FileReader();
                reader.readAsArrayBuffer(selectedFile)
                reader.onload = (e) => {
                    setExcelFile(e.target.result)
                    setExcelFileName(selectedFile.name)
                }
            } else {
                setTypeError(ERROR_FILE_TYPE)
                setExcelFile(null)
                setExcelFileName(null)
                setExcelData(null)
                setUploadResult(null)
            }
        } else {
            setTypeError(ERROR_FILE_EMPTY)
            setExcelFile(null)
            setExcelFileName(null)
            setExcelData(null)
            setUploadResult(null)
        }
    }

    const handlePreview = (e) => {
        e.preventDefault()
        if (excelFile !== null) {
            const workbook = XLSX.read(excelFile, { type: 'buffer' })
            const worksheetName = workbook.SheetNames[0]
            const worksheet = workbook.Sheets[worksheetName]
            const data = XLSX.utils.sheet_to_json(worksheet)
            setExcelData(data)
        } else {
            setTypeError(ERROR_FILE_EMPTY)
            setExcelFile(null)
            setExcelFileName(null)
            setExcelData(null)
            setUploadResult(null)
        }
    }

    const handleFileSubmit = (e) => {
        e.preventDefault()
        // call post api
        requestPost()
    }

    const renderPreview = () => {
        return <div className="card card-primary">
            <div className="card-header">
                {excelFileName}
            </div>
            <div className="card-body table-responsive p-0">
                <table className="table text-nowrap table-sticky">
                    <thead>
                        <tr>
                            {Object.keys(excelData[0]).map((key) => (
                                <th key={key}>{key}</th>))}
                        </tr>
                    </thead>
                    <tbody>
                        {excelData.map((data, index) => (
                            <tr key={index}>
                                {Object.values(data).map((value, index) => (
                                    <td key={index}>{value}</td>))}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    }

    return <>
        {excelData ? (renderPreview()) : (<></>)}
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
    </>
    /*
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