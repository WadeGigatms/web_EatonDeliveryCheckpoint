import React from 'react'
import {
    TABLE_CARGO_NO,
    TABLE_MATERIAL_QTY,
    TABLE_PRODUCT_QTY,
    TABLE_UPLOAD_TIME,
    TABLE_MATERIAL,
    TABLE_TARGET_MATERIAL_QTY,
    TABLE_REALTIME_MATERIAL_QTY,
    TABLE_REALTIME_PALLET_QTY,
    TABLE_DATE,
    TABLE_START_TIME,
    TABLE_END_TIME,
    TABLE_DURATION,
    TABLE_VALID_PALLET_QTY,
    TABLE_INVALID_PALLET_QTY
} from '../constants'

const CargoContent = ({ cargoNos }) => {

    return <div className="card card-primary">
        <div className="card-header">上傳檔案</div>
        <div className="card-body table-responsive p-0">
            <table className="table text-nowrap table-sticky">
                <thead>
                    <tr>
                        <th>{TABLE_CARGO_NO}</th>
                        <th>{TABLE_MATERIAL_QTY}</th>
                        <th>{TABLE_PRODUCT_QTY}</th>
                    </tr>
                </thead>
                <tbody>
                    {cargoNos ? (
                        cargoNos.map((cargono, index) => (
                            <tr key={index}>
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