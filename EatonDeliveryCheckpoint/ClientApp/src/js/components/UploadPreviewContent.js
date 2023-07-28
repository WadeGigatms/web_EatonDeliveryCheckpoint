import React from "react";
import {
    TABLE_PREVIEW_DATA
} from '../constants'

const UploadPreviewContent = ({ fileName, fileData }) => {
    return <div className="card card-primary h-100">
        <div className="card-header">
            {TABLE_PREVIEW_DATA} {fileName}
        </div>
        <div className="card-body table-responsive p-0">
            <table className="table text-nowrap table-sticky">
                <thead>
                    <tr>
                        {fileData && Object.keys(fileData[0]).map((key) => (
                            <th key={key}>{key}</th>))}
                    </tr>
                </thead>
                <tbody>
                    {fileData && fileData.map((data, index) => (
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

export default UploadPreviewContent