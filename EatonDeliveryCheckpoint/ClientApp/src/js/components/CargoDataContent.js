import React, { useState, useEffect } from 'react'
import {
    TABLE_UPLOADED_MATERIAL,
    TABLE_MATERIAL,
    TABLE_TARGET_MATERIAL_QTY,
    TABLE_REALTIME_MATERIAL_QTY,
    TABLE_REALTIME_PALLET_QTY,
} from '../constants'

const CargoDataContent = ({ cargoDatas }) => {

    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_UPLOADED_MATERIAL}</div>
        <div className="card-body table-responsive p-0">
            <table className="table text-nowrap table-sticky">
                <thead>
                    <tr>
                        <th>{TABLE_MATERIAL}</th>
                        <th>{TABLE_TARGET_MATERIAL_QTY}</th>
                        <th>{TABLE_REALTIME_MATERIAL_QTY}</th>
                        <th>{TABLE_REALTIME_PALLET_QTY}</th>
                    </tr>
                </thead>
                <tbody>
                    
                </tbody>
            </table>
        </div>
        <div className="card-footer"></div>
    </div>
}

export default CargoDataContent