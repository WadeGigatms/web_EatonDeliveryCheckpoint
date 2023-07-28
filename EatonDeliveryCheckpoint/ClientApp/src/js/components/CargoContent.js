import React, { useState, useEffect } from 'react';
import { FormControlLabel, Checkbox } from '@mui/material';
import {
    TABLE_UPLOADED_DN,
    TABLE_CARGO_NO,
    TABLE_MATERIAL_QTY,
    TABLE_PRODUCT_QTY,
} from '../constants'

const CargoContent = ({ cargoNos, editState }) => {
    const [checkedArray, setCheckedArray] = useState([false])

    useEffect(() => {
        if (cargoNos) {
            const boolArray = cargoNos.map(() => { return false })
            setCheckedArray(boolArray)
            console.log(boolArray)
        }
    }, [cargoNos])

    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_UPLOADED_DN}</div>
        <div className="card-body table-responsive p-0">
            <table className="table text-nowrap table-sticky">
                <thead>
                    <tr>
                        {editState && <th></th> }
                        <th>{TABLE_CARGO_NO}</th>
                        <th>{TABLE_MATERIAL_QTY}</th>
                        <th>{TABLE_PRODUCT_QTY}</th>
                    </tr>
                </thead>
                <tbody>
                    {cargoNos ? (
                        cargoNos.map((cargono, index) => (
                            <tr key={index}>
                                {editState &&
                                    <FormControlLabel
                                        control={<Checkbox checked={checkedArray[index]} onChange={handleChange} />}
                                    />}
                                <th>{cargono.no}</th>
                                <th>{cargono.material_quantity}</th>
                                <th>{cargono.product_quantity}</th>
                            </tr>
                        ))
                        ) : (<></>)}
                </tbody>
            </table>
        </div>
        <div className="card-footer"></div>
    </div>
}

export default CargoContent