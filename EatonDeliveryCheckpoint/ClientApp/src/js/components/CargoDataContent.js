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
    TABLE_MATERIAL,
    TABLE_TARGET_MATERIAL_QTY,
    TABLE_REALTIME_MATERIAL_QTY,
    TABLE_REALTIME_PALLET_QTY,
} from '../constants';

const CargoDataContent = ({ className, title, deliveryStep, datas }) => {

    return <div className={"card card-primary " + className }>
        <div className="card-header">{title}</div>
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
                            datas ? datas.map((row, index) => {
                                // 0: unchecked, 1: checked, 2: deliverying, 3: finish and review, -1: alert or pause
                                // Set classname
                                var classname = ""
                                if (deliveryStep === 2) {
                                    if (row.alert === 1) {
                                        classname = "lightcoral"
                                    } else if (row.count === row.realtime_product_count) {
                                        classname = "lime"
                                    }
                                } else if (deliveryStep === 3) {
                                    classname = row.alert === 1 || row.count > row.realtime_product_count ? "lightcoral" : "lime"
                                }

                                // Visible
                                if (deliveryStep === 2 && row.alert === 0 && row.count === -1) {
                                    return <TableRow key={index}></TableRow>
                                } else {
                                    return <TableRow sx={{ backgroundColor: classname }} key={index}>
                                        <TableCell>{row.material}</TableCell>
                                        <TableCell align="right">{row.count}</TableCell>
                                        <TableCell align="right">{deliveryStep > 1 ? row.realtime_product_count : "-"}</TableCell>
                                        <TableCell align="right">{deliveryStep > 1 ? row.realtime_pallet_count : "-"}</TableCell>
                                    </TableRow>
                                }
                            }) : <></>
                        }
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    </div>
}

export default CargoDataContent