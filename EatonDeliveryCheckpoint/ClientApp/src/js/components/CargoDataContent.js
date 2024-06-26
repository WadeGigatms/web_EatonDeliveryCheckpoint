﻿import React, { useState, useEffect } from 'react';
import {
    TableContainer,
    Table,
    TableHead,
    TableBody,
    TableRow,
    TableCell,
    Paper,
    Chip
} from '@mui/material';
import {
    TABLE_MATERIAL,
    TABLE_TARGET_MATERIAL_QTY,
    TABLE_REALTIME_MATERIAL_QTY,
    TABLE_TARGET_PALLET_QTY,
    TABLE_REALTIME_PALLET_QTY,
} from '../constants';
import { usePrevious } from '../hooks/usePrevious';
import { useStyles } from '../mui/useStyles';

const CargoDataContent = ({ className, title, deliveryStep, datas }) => {
    const [highlightIndex, setHighlightIndex] = useState(-1)
    const previousDatas = usePrevious(datas)
    const { blinker, invalidBlinker } = useStyles()

    useEffect(() => {
        if (deliveryStep === "delivery" && previousDatas !== undefined && previousDatas !== null && datas !== null) {
            if (previousDatas.length !== datas.length) {
                setHighlightIndex(datas.length - 1)
            } else {
                for (var i = 0; i < datas.length; i++) {
                    if (previousDatas[i].realtime_pallet_count < datas[i].realtime_pallet_count) {
                        setHighlightIndex(i)
                        return
                    }
                }
            }
        }
    }, [deliveryStep, datas, previousDatas])

    const displayReaderAndPallet = (step, row) => {
        if (step === "delivery" && row.records) {
            if (row.records.length > 0) {
                var readerId = deliveryStep === "delivery" && row.records.length > 0 ? row.records[row.records.length - 1].reader_id : ""
                if (readerId === "TerminalLeft") {
                    readerId = "L"
                } else if (readerId === "TerminalRight") {
                    readerId = "R"
                }
                var pallet_id = deliveryStep === "delivery" && row.records.length > 0 ? row.records[row.records.length - 1].pallet_id : ""

                return <>
                    <Chip label={readerId} size="small" />
                    <Chip label={pallet_id} size="small" />
                </>
            }
        }
        return <></>
    }

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
                                if (deliveryStep === "delivery") {
                                    if (row.product_count === row.realtime_product_count) {
                                        backgroundColor = "lime"
                                    } else if (row.alert === 1 || row.product_count < row.realtime_product_count) {
                                        backgroundColor = "lightCoral"
                                    } else {
                                        backgroundColor = ""
                                    }

                                    if (index === highlightIndex) {
                                        classname = row.alert === 0 ? `${blinker}` : `${invalidBlinker}`
                                    } else {
                                        classname = ""
                                    }
                                } else if (deliveryStep === "finish") {
                                    if (row.product_count === row.realtime_product_count) {
                                        backgroundColor = "lime"
                                    } else if (row.alert === 1 || row.product_count > row.realtime_product_count) {
                                        backgroundColor = "lightCoral"
                                    }
                                }

                                // Get last reader_id, pallet_id from data
                                if (row.records !== null) {
                                    var readerId = deliveryStep === "delivery" && row.records.length > 0 ? row.records[row.records.length - 1].reader_id : ""
                                    if (readerId === "TerminalLeft") {
                                        readerId = "L"
                                    } else if (readerId === "TerminalRight") {
                                        readerId = "R"
                                    }
                                    var pallet_id = deliveryStep === "delivery" && row.records.length > 0 ? row.records[row.records.length - 1].pallet_id : ""
                                }

                                // Visible
                                if (deliveryStep === "delivery" && row.alert === 0 && row.product_count === -1) {
                                    return <TableRow key={index}></TableRow>
                                } else {
                                    return <TableRow sx={{ backgroundColor: backgroundColor }} className={classname} key={index} >
                                        <TableCell>
                                            {row.material} {displayReaderAndPallet(deliveryStep, row)}
                                        </TableCell>
                                        <TableCell align="right">{row.product_count}</TableCell>
                                        <TableCell align="right">{deliveryStep === "delivery" || deliveryStep === "alert" || deliveryStep === "finish" ? row.realtime_product_count : "-"}</TableCell>
                                        <TableCell align="right">{row.pallet_count}</TableCell>
                                        <TableCell align="right">{deliveryStep === "delivery" || deliveryStep === "alert" || deliveryStep === "finish" ? row.realtime_pallet_count : "-"}</TableCell>
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