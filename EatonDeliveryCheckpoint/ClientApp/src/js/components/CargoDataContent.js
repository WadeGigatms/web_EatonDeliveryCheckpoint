import React, { useState, useEffect } from 'react';
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
    TABLE_UPLOADED_MATERIAL,
    TABLE_MATERIAL,
    TABLE_TARGET_MATERIAL_QTY,
    TABLE_REALTIME_MATERIAL_QTY,
    TABLE_REALTIME_PALLET_QTY,
} from '../constants';

const CargoDataContent = ({ deliveryStep, selectedCargoNo }) => {
    useEffect(() => {
        if (selectedCargoNo) {
            console.log(selectedCargoNo.no)
        }
    }, [selectedCargoNo])

    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_UPLOADED_MATERIAL}</div>
        <div className="card-body table-responsive p-0">
            <TableContainer component={Paper}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>{TABLE_MATERIAL}</TableCell>
                            <TableCell align="right">{TABLE_TARGET_MATERIAL_QTY}</TableCell>
                            <TableCell align="right">{TABLE_REALTIME_MATERIAL_QTY}</TableCell>
                            <TableCell align="right">{TABLE_REALTIME_PALLET_QTY}</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            selectedCargoNo ? selectedCargoNo.datas.map((row, index) => {
                                var classname = ""
                                if (row.count == row.realtime_product_count) {
                                    classname = "lightgreen"
                                } else if (row.count < row.realtime_product_count) {
                                    classname = "red"
                                }

                                return (
                                    <TableRow
                                        sx={{ backgroundColor: classname }}
                                        key={index} color="">
                                        <TableCell>{row.material}</TableCell>
                                        <TableCell align="right">{row.count}</TableCell>
                                        <TableCell align="right">{deliveryStep > 1 ? row.realtime_product_count : "-"}</TableCell>
                                        <TableCell align="right">{deliveryStep > 1 ? row.realtime_pallet_count : "-"}</TableCell>
                                    </TableRow>
                                )
                            }) : <></>
                        }
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    </div>
}

export default CargoDataContent