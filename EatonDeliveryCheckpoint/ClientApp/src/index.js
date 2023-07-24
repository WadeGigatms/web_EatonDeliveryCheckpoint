import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import registerServiceWorker from './registerServiceWorker';

import '../node_modules/admin-lte/plugins/bootstrap/js/bootstrap.bundle.min.js';
import '../node_modules/admin-lte/plugins/jquery/jquery.min.js';
import '../node_modules/admin-lte/plugins/toastr/toastr.min.js';
import '../node_modules/admin-lte/dist/js/adminlte.min.js';

import '../node_modules/admin-lte/plugins/fontawesome-free/css/all.min.css';
import '../node_modules/admin-lte/plugins/toastr/toastr.min.css';
import '../node_modules/admin-lte/dist/css/adminlte.min.css';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement);

registerServiceWorker();

