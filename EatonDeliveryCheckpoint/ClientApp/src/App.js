import React from 'react';
import { Route, Switch } from 'react-router-dom';
import Layout from './js/components/Layout';
import { Home } from './js/components/Home';
import { VERSION } from './js/constants';

import './css/custom.css';

const App = () => {
    return <Layout version={VERSION}>
		<Switch>
			<Route exact path="/" component={Home} />
		</Switch>
	</Layout>
}

export default App