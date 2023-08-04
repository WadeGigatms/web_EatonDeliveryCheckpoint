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
    TABLE_UPLOADED_DATA,
    TABLE_DATE,
    TABLE_START_TIME,
    TABLE_END_TIME,
    TABLE_DURATION,
    TABLE_VALID_PALLET_QTY,
    TABLE_INVALID_PALLET_QTY
} from '../constants';

const CargoRealtimeContent = ({ deliveryStage, cargoNo }) => {
    const [dn, setDN] = useState(null)

    useEffect(() => {
        if (deliveryStage > 1 && cargoNo) {
            setDN(cargoNo)
        }
    }, [deliveryStage, cargoNo])

    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_UPLOADED_DATA}</div>
        <div className="card-body table-responsive p-0">
            <TableContainer component={Paper}>
                <Table size="small">
                    <TableBody>
                        <TableRow>
                            <TableCell>{TABLE_DATE}</TableCell>
                            <TableCell align="right">{dn ? dn.date : "-"}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_START_TIME}</TableCell>
                            <TableCell align="right">{dn ? dn.start_time : "-"}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_END_TIME}</TableCell>
                            <TableCell align="right">{dn ? dn.end_time : "-"}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_DURATION}</TableCell>
                            <TableCell align="right">{dn ? dn.duration : "-"}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_VALID_PALLET_QTY}</TableCell>
                            <TableCell align="right">{dn ? dn.valid_pallet_quantity : "-"}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_INVALID_PALLET_QTY}</TableCell>
                            <TableCell align="right">{dn ? dn.invalid_pallet_quantity : "-"}</TableCell>
                        </TableRow>
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    </div>
}

export default CargoRealtimeContent