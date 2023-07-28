import React, { useState, useEffect } from 'react';
import { Stack, Button } from '@mui/material';
import CargoContent from './CargoContent';
import CargoDataContent from './CargoDataContent';
import CargoForm from './CargoForm';
import CargoRealtimeContent from './CargoRealtimeContent';

const DeliveryDashboard = ({ cargoNos }) => {
    const [editState, setEditState] = useState(false)
    const [cargoDatas, setCargoDatas] = useState()

    useEffect(() => {

    }, [cargoNos])


    return <div className="row h-100 p-3">
        <div className="col-sm-3 h-100">
            <CargoContent cargoNos={cargoNos} editState={editState} />
        </div>
        <div className="col-sm-6 h-100">
            <CargoDataContent cargoDatas={cargoDatas} />
        </div>
        <div className="col-sm-3 h-100">
            <Stack spacing={1} direction="column">
                <CargoRealtimeContent  />
                <Button variant="contained"></Button>
            </Stack>
        </div>
    </div>
}

export default DeliveryDashboard