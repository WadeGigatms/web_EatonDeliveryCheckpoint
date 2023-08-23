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

const MuiFormDialog = ({ open, onClose, value, onChange, title, contentText, helperText, primaryButton, handlePrimaryButtonClick }) => {

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
                error={helperText === "" ? false : true}
                helperText={helperText}
                value={value}
                onChange={onChange} />
        </DialogContent>
        <DialogActions>
            <Button
                fullWidth
                variant="contained"
                onClick={handlePrimaryButtonClick}>{primaryButton}</Button>
        </DialogActions>
    </Dialog>
}

export default MuiFormDialog