import React, { useState, useEffect } from 'react';
import {
    TableContainer,
    Table,
    TableBody,
    TableRow,
    TableCell,
    Paper
} from '@mui/material';
import {
    TABLE_NO,
    TABLE_REALTIME_DATA,
    TABLE_DATE,
    TABLE_START_TIME,
    TABLE_END_TIME,
    TABLE_DURATION,
    TABLE_PALLET_QTY,
    TABLE_MISS_PALLET_QTY,
    TABLE_VALID_PALLET_QTY,
    TABLE_INVALID_PALLET_QTY,
    TABLE_PALLET_RATE,

} from '../constants';
import moment from 'moment';

const CargoDataInfoContent = ({ deliveryStep, selectedDeliveryCargoDto }) => {
    const [no, setNo] = useState("-")
    const [date, setDate] = useState("-")
    const [startTime, setStartTime] = useState("-")
    const [endTime, setEndTime] = useState("-")
    const [duration, setDuration] = useState("-")
    const [palletQuantity, setPalletQuantity] = useState("-")
    const [missPalletQuantity, setMissPalletQuantity] = useState("-")
    const [validPalletQuantity, setValidPalletQuantity] = useState("-")
    const [invalidPalletQuantity, setInvalidPalletQuantity] = useState("-")
    const [rate, setRate] = useState("-")
    const [currentTime, setCurrentTime] = useState(moment());

    useEffect(() => {
        const interval = setInterval(() => {
            setCurrentTime(moment());
        }, 1000);

        const isDelivery = deliveryStep === "delivery" || deliveryStep === "alert" || deliveryStep === "finish" ? true : false

        if (isDelivery === true && selectedDeliveryCargoDto) {
            const startDate = new Date(selectedDeliveryCargoDto.start_time)
            const endDate = selectedDeliveryCargoDto.state === "finish" ? new Date(selectedDeliveryCargoDto.end_time) : currentTime
            const startMoment = moment(startDate)
            const endMoment = selectedDeliveryCargoDto.state === "finish" ? moment(endDate) : currentTime
            const date = startMoment.format("yyyy/MM/DD")
            const startTime = startMoment.format("HH:mm:ss")
            const endTime = endMoment.format("HH:mm:ss")
            const duration = moment.duration(endMoment.diff(startMoment))
            const durationTime = String(duration.hours()).padStart(2, "0") + ":" + String(duration.minutes()).padStart(2, "0") + ":" + String(duration.seconds()).padStart(2, "0")
            const rate = Math.round(100 * (selectedDeliveryCargoDto.pallet_quantity - selectedDeliveryCargoDto.miss_pallet_quantity) / selectedDeliveryCargoDto.pallet_quantity)

            setNo(selectedDeliveryCargoDto.no)
            setDate(date)
            setStartTime(startTime)
            setEndTime(endTime)
            setDuration(durationTime)
            setPalletQuantity(selectedDeliveryCargoDto.pallet_quantity)
            setMissPalletQuantity(selectedDeliveryCargoDto.miss_pallet_quantity)
            setValidPalletQuantity(parseFloat(selectedDeliveryCargoDto.valid_pallet_quantity))
            setInvalidPalletQuantity(parseFloat(selectedDeliveryCargoDto.invalid_pallet_quantity))
            setRate(rate + "%")
        } else if (selectedDeliveryCargoDto === null || selectedDeliveryCargoDto === undefined) {
            setNo("-")
            setDate("-")
            setStartTime("-")
            setEndTime("-")
            setDuration("-")
            setPalletQuantity("-")
            setMissPalletQuantity("-")
            setValidPalletQuantity("-")
            setInvalidPalletQuantity("-")
            setRate("-")
        }

        return () => clearInterval(interval)
    }, [deliveryStep, selectedDeliveryCargoDto, currentTime])

    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_REALTIME_DATA}</div>
        <div className="card-body table-responsive p-0">
            <TableContainer component={Paper}>
                <Table size="small">
                    <TableBody>
                        <TableRow>
                            <TableCell>{TABLE_NO}</TableCell>
                            <TableCell align="right">{no}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_DATE}</TableCell>
                            <TableCell align="right">{date}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_START_TIME}</TableCell>
                            <TableCell align="right">{startTime}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_END_TIME}</TableCell>
                            <TableCell align="right">{endTime}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_DURATION}</TableCell>
                            <TableCell align="right">{duration}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_PALLET_QTY}</TableCell>
                            <TableCell align="right">{palletQuantity}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_MISS_PALLET_QTY}</TableCell>
                            <TableCell align="right">{missPalletQuantity}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_VALID_PALLET_QTY}</TableCell>
                            <TableCell align="right">{validPalletQuantity}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_INVALID_PALLET_QTY}</TableCell>
                            <TableCell align="right">{invalidPalletQuantity}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell>{TABLE_PALLET_RATE}</TableCell>
                            <TableCell align="right">{rate}</TableCell>
                        </TableRow>
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    </div>
}

export default CargoDataInfoContent