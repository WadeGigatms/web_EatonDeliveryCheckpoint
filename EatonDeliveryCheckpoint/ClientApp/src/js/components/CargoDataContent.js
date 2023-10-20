import React, { useState, useEffect } from 'react';
import {
    TableContainer,
    Table,
    TableHead,
    TableBody,
    TableRow,
    TableCell,
    Paper,
} from '@mui/material';
import {
    TABLE_MATERIAL,
    TABLE_TARGET_MATERIAL_QTY,
    TABLE_REALTIME_MATERIAL_QTY,
    TABLE_TARGET_PALLET_QTY,
    TABLE_REALTIME_PALLET_QTY,
} from '../constants';
import { usePrevious } from '../hooks/usePrevious';
import { useStyles } from '../hooks/useStyles';

const CargoDataContent = ({ className, title, deliveryStep, datas }) => {
    const [highlightRow, setHighlightRow] = useState(-1)
    const previousDatas = usePrevious(datas)
    const { blinker, invalidBlinker } = useStyles()

    useEffect(() => {
        console.log(datas)
        console.log(previousDatas)
        if (deliveryStep === 2 && previousDatas !== undefined && previousDatas !== null) {
            if (previousDatas.length !== datas.length) {
                setHighlightRow(datas.length - 1)
                console.log("BANG")
            } else {
                for (var i = 0; i < datas.length; i++) {
                    if (previousDatas[i].realtime_pallet_count < datas[i].realtime_pallet_count) {
                        setHighlightRow(i)
                        console.log("BANG BANG")
                        console.log(i)
                        return
                    }
                }
            }
        }
    }, [deliveryStep, datas])

    return <div className={"card card-primary " + className}>
        <div className="card-header">{title}</div>
        <div className="card-body table-responsive p-0">
            <TableContainer component={Paper}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>{TABLE_MATERIAL}</TableCell>
                            <TableCell align="right">{TABLE_TARGET_MATERIAL_QTY}</TableCell>
                            <TableCell align="right">{TABLE_REALTIME_MATERIAL_QTY}</TableCell>
                            <TableCell align="right">{TABLE_TARGET_PALLET_QTY}</TableCell>
                            <TableCell align="right">{TABLE_REALTIME_PALLET_QTY}</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            datas ? datas.map((row, index) => {
                                // 0: new, 1: select, 2: deliverying, 3: finish/search/review, -1: alert/pause
                                // Set classname
                                var backgroundColor = ""
                                var classname = ""
                                if (deliveryStep === 2) {
                                    if (row.product_count === row.realtime_product_count) {
                                        backgroundColor = "lime"
                                    } else if (row.alert === 1 || row.product_count < row.realtime_product_count) {
                                        backgroundColor = "lightCoral"
                                    } else {
                                        backgroundColor = ""
                                    }

                                    if (index === highlightRow) {
                                        classname = row.alert === 0 ? `${blinker}` : `${invalidBlinker}`
                                        console.log(classname)
                                    } else {
                                        classname = ""
                                    }
                                } else if (deliveryStep === 3) {
                                    if (row.product_count === row.realtime_product_count) {
                                        backgroundColor = "lime"
                                    } else if (row.alert === 1 || row.product_count > row.realtime_product_count) {
                                        backgroundColor = "lightCoral"
                                    }
                                }

                                // Visible
                                if (deliveryStep === 2 && row.alert === 0 && row.product_count === -1) {
                                    return <TableRow key={index}></TableRow>
                                } else {
                                    return <TableRow sx={{ backgroundColor: backgroundColor }} className={classname} key = { index } >
                                        <TableCell>{row.material}</TableCell>
                                        <TableCell align="right">{row.product_count}</TableCell>
                                        <TableCell align="right">{deliveryStep > 1 ? row.realtime_product_count : "-"}</TableCell>
                                        <TableCell align="right">{row.pallet_count}</TableCell>
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