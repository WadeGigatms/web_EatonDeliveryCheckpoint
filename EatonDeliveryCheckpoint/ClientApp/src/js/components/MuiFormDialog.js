import React from 'react';
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    TextField
} from '@mui/material';

const MuiFormDialog = ({ open, onClose, value, onChange, title, contentText, primaryButton, secondaryButton, handlePrimaryButtonClick, handleSecondaryButtonClick }) => {

    return <Dialog open={open} onClose={onClose} fullWidth={true} maxWidth="xs">
        <DialogTitle>{title}</DialogTitle>
        <DialogContent>
            <DialogContentText>{contentText}</DialogContentText>
            <TextField
                required
                autoFocus
                fullWidth
                label="DN"
                variant="standard"
                value={value}
                onChange={onChange} />
        </DialogContent>
        <DialogActions>
            <Button variant="contained" onClick={handlePrimaryButtonClick}>{primaryButton}</Button>
            <Button variant="text" onClick={handleSecondaryButtonClick}>{secondaryButton}</Button>
        </DialogActions>
    </Dialog>
}

export default MuiFormDialog