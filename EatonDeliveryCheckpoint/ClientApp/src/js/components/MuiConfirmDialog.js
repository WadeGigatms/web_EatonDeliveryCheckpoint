import React from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from '@mui/material';
import {
    BTN_CONFIRM,
    BTN_CANCEL,
} from '../constants';

const MuiConfirmDialog = ({ open, title, contentText, handlePrimaryButtonClick, handleSecondaryButtonClick }) => {

    return <Dialog open={open} fullWidth={true} maxWidth="xs">
        <DialogTitle>{title}</DialogTitle>
        <DialogContent>
            <DialogContentText>{contentText}</DialogContentText>
        </DialogContent>
        <DialogActions>
            <Button variant="contained" onClick={handlePrimaryButtonClick}>{BTN_CONFIRM}</Button>
            <Button variant="text" onClick={handleSecondaryButtonClick}>{BTN_CANCEL}</Button>
        </DialogActions>
    </Dialog>
}

export default MuiConfirmDialog