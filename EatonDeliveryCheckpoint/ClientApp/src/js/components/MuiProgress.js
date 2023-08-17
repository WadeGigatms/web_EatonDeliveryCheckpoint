import React from 'react';
import {
    Dialog,
    DialogContent,
    DialogContentText,
    CircularProgress,
} from '@mui/material';

const MuiProgress = ({ open }) => {

    return <Dialog open={open} >
        <DialogContent>
            <CircularProgress variant="indeterminate" disableShrink={true} size="4rem" />
            <DialogContentText>載入中</DialogContentText>
        </DialogContent>
    </Dialog>
}

export default MuiProgress