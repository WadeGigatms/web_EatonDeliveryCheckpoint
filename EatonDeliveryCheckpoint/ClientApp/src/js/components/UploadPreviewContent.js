import React from "react";
import {
    TableContainer,
    Table,
    TableHead,
    TableBody,
    TableRow,
    TableCell,
    Paper
} from '@mui/material';
import {
    TABLE_PREVIEW_FILE
} from '../constants';

const UploadPreviewContent = ({ fileData }) => {
    return <div className="card card-primary h-100">
        <div className="card-header">
            {TABLE_PREVIEW_FILE}
        </div>
        <div className="card-body table-responsive p-0">
            <TableContainer component={Paper}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            {
                                fileData && Object.keys(fileData[0]).map((key) => (
                                    <TableCell key={key}>{key}</TableCell>
                                ))
                            }
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            fileData && fileData.map((row, index) => (
                                <TableRow key={index}>
                                    {Object.values(row).map((value, index) => (
                                        <TableCell key={index}>{value}</TableCell>))}
                                </TableRow>
                            ))
                        }
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    </div>
}

export default UploadPreviewContent