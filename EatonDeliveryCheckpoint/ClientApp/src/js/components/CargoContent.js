import React, { useState, useEffect } from 'react';
import {
    TableContainer,
    Table,
    TableHead,
    TableBody,
    TableRow,
    TableCell,
    Paper,
    Checkbox
} from '@mui/material';
import {
    TABLE_UPLOADED_DN,
    TABLE_CARGO_NO,
    TABLE_MATERIAL_QTY,
    TABLE_PRODUCT_QTY,
} from '../constants'

const CargoContent = ({ deliveryStep, deliveryNumberDtos, selectedDeliveryNumberDto, setSelectedDeliveryNumberDto }) => {
    const [selectedNo, setSelectedNo] = useState("")

    useEffect(() => {
        if (selectedDeliveryNumberDto) {
            setSelectedNo(selectedDeliveryNumberDto.no)
        } else {
            setSelectedNo("")
        }
    }, [selectedDeliveryNumberDto])

    const handleTableRowClick = (e, index, no) => {
        if (deliveryStep === "select" || deliveryStep === "edit") {
            if (selectedNo !== no) {
                setSelectedNo(no)
                setSelectedDeliveryNumberDto(deliveryNumberDtos[index])
            } else {
                setSelectedNo(-1)
                setSelectedDeliveryNumberDto(null)
            }
        }
    }

    function isSelected(no) {
        return selectedNo === no ? true : false
    }

    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_UPLOADED_DN}</div>
        <div className="card-body table-responsive p-0">
            <TableContainer component={Paper}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell></TableCell>
                            <TableCell>{TABLE_CARGO_NO}</TableCell>
                            <TableCell align="right">{TABLE_MATERIAL_QTY}</TableCell>
                            <TableCell align="right">{TABLE_PRODUCT_QTY}</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            deliveryNumberDtos ? deliveryNumberDtos.map((row, index) => (
                                <TableRow
                                    hover
                                    onClick={(e) => handleTableRowClick(e, index, row.no)}
                                    role="checkbox"
                                    key={index}
                                    selected={isSelected(row.no)}
                                    sx={{ cursor: 'pointer' }} >
                                    {
                                        deliveryStep !== "new" ? (
                                            <TableCell padding="checkbox">
                                                <Checkbox checked={isSelected(row.no)} />
                                            </TableCell>) : <TableCell></TableCell>
                                    }
                                    <TableCell>{row.no}</TableCell>
                                    <TableCell align="right">{row.material_quantity}</TableCell>
                                    <TableCell align="right">{row.product_quantity}</TableCell>
                                </TableRow>
                            )) : <></>
                        }
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    </div>
}

export default CargoContent