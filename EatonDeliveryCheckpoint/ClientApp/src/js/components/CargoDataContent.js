import React from 'react';
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

const CargoDataContent = ({ deliveryStep, selectedDeliveryCargoDto }) => {

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
                            selectedDeliveryCargoDto ? selectedDeliveryCargoDto.datas.map((row, index) => {
                                var classname = ""
                                // 0: unchecked, 1: checked, 2: deliverying, 3: finish and review, -1: alert or pause
                                if (row.alert === 1) {
                                    classname = "red"
                                } else if (row.count === row.realtime_product_count) {
                                    classname = "lightgreen"
                                }

                                if (deliveryStep === 2 && row.count == row.realtime_product_count) {
                                    return <></>
                                }

                                return (
                                    <TableRow
                                        sx={{ backgroundColor: classname }}
                                        key={index}>
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