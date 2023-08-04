﻿import {
    Button,
    Alert,
    AlertTitle,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
} from '@mui/material';
import {
    BTN_CONFIRM,
} from '../constants';

const MuiAlertDialog = ({ severity, open, title, contentText, handleButtonClick }) => {

    return <Dialog open={open} fullWidth={true} maxWidth="xs">
        <DialogTitle>{title}</DialogTitle>
        <Alert variant="filled" severity={severity}>
            {contentText}
        </Alert>
        <DialogActions>
            <Button variant="contained" onClick={handleButtonClick}>{BTN_CONFIRM}</Button>
        </DialogActions>
    </Dialog>
}

export default MuiAlertDialog