import React from 'react';
import { Route, Switch } from 'react-router-dom';
import Content from './js/components/Content';
import { VERSION } from './js/constants';

import './css/custom.css';

const App = () => {
    return <Content version={VERSION} />
}

export default App