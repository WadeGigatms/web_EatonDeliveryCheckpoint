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
            <DialogContentText>Loading..</DialogContentText>
        </DialogContent>
    </Dialog>
}

export default MuiProgress