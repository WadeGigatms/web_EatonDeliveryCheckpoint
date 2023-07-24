import React from 'react';

const ExcelData = ({ excelData }) => {
    return excelData.map((data, index) => (
        <tr key={index}>
            <th>{data.Delivery}</th>
            <th>{data.Item}</th>
            <th>{data.Material}</th>
            <th>{data.Quantity}</th>
            <th>{data.No}</th>
        </tr>
    ))
}

export default ExcelData