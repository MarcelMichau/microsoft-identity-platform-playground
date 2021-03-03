import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

import { MsalProvider } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';

const configuration = {
	auth: {
		clientId: '2aedb97c-4831-4e06-936d-b79358d576f8',
		authority:
			'https://login.microsoftonline.com/f75f1009-f6f1-4ee7-a028-372b490c585b/',
	},
};

const pca = new PublicClientApplication(configuration);

ReactDOM.render(
	<React.StrictMode>
		<MsalProvider instance={pca}>
			<App />
		</MsalProvider>
	</React.StrictMode>,
	document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
