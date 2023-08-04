import React from 'react';
import Content from './js/components/Content';
import { VERSION } from './js/constants';
import './css/custom.css';
import './css/mui.css';
import { createTheme, colors, ThemeProvider } from '@mui/material';

const theme = createTheme({
    palette: {
        secondary: {
            main: colors.grey[500],
            contrastText: colors.grey[50],
        },
    },
})
const App = () => {
    return <ThemeProvider theme={theme}>
        <Content version={VERSION} />
    </ThemeProvider>
}

export default App      