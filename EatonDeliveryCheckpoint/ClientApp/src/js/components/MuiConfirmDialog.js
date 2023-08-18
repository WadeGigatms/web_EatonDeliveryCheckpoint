import React from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from '@mui/material';

const MuiConfirmDialog = ({ open, onClose, title, contentText, primaryButton, secondaryButton, handlePrimaryButtonClick, handleSecondaryButtonClick }) => {

    return <Dialog open={open} onClose={onClose} fullWidth={true} maxWidth="xs">
        <DialogTitle>{title}</DialogTitle>
        <DialogContent>
            <DialogContentText>{contentText}</DialogContentText>
        </DialogContent>
        <DialogActions>
            <Button variant="contained" onClick={handlePrimaryButtonClick}>{primaryButton}</Button>
            <Button variant="text" onClick={handleSecondaryButtonClick}>{secondaryButton}</Button>
        </DialogActions>
    </Dialog>
}

export default MuiConfirmDialog