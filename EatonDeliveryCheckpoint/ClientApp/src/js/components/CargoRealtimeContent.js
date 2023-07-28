import React, { useState, useEffect } from 'react';
import {
    TABLE_UPLOADED_DATA,
    TABLE_DATE,
    TABLE_START_TIME,
    TABLE_END_TIME,
    TABLE_DURATION,
    TABLE_VALID_PALLET_QTY,
    TABLE_INVALID_PALLET_QTY
} from '../constants';

const CargoRealtimeContent = ({ cargoNo }) => {
    return <div className="card card-primary h-100">
        <div className="card-header">{TABLE_UPLOADED_DATA}</div>
        <div className="card-body table-responsive p-0">
            <table className="table text-nowrap table-sticky">
                <tbody>
                    <tr>
                        <th>{TABLE_DATE}</th>
                        <td>{cargoNo && cargoNo.date}</td>
                    </tr>
                    <tr>
                        <th>{TABLE_START_TIME}</th>
                        <td>{cargoNo && cargoNo.start_time}</td>
                    </tr>
                    <tr>
                        <th>{TABLE_END_TIME}</th>
                        <td>{cargoNo && cargoNo.end_time}</td>
                    </tr>
                    <tr>
                        <th>{TABLE_DURATION}</th>
                        <td>{cargoNo && cargoNo.duration}</td>
                    </tr>
                    <tr>
                        <th>{TABLE_VALID_PALLET_QTY}</th>
                        <td>{cargoNo && cargoNo.valid_pallet_quantity}</td>
                    </tr>
                    <tr>
                        <th>{TABLE_INVALID_PALLET_QTY}</th>
                        <td>{cargoNo && cargoNo.invalid_pallet_quantity}</td>
                    </tr>
                </tbody>
            </table>
        </div>
        <div className="card-footer"></div>
    </div>
}

export default CargoRealtimeContent