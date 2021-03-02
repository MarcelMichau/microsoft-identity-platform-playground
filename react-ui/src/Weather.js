import React, { useState } from 'react';
import { useMsal, useAccount } from '@azure/msal-react';

const Weather = () => {
	const { instance, accounts } = useMsal();
	const account = useAccount(accounts[0]);
	const [weatherInfo, setWeatherInfo] = useState([]);

	const fetchData = async () => {
		const apiUrl = 'https://localhost:44353/WeatherForecast';

		if (account) {
			const response = await instance.acquireTokenSilent({
				scopes: ['api://73e9f49d-c747-4443-b973-74288ac6e6d8/Api.ReadWrite'],
				account: account,
			});
			if (response) {
				const res = await fetch(apiUrl, {
					headers: { Authorization: `Bearer ${response.accessToken}` },
				});
				const data = await res.json();
				setWeatherInfo(data);
			}
		}
	};

	return (
		<div>
			<button onClick={() => fetchData()}>Get Weather From API</button>

			<pre>{JSON.stringify(weatherInfo, null, '\t')}</pre>
		</div>
	);
};
export default Weather;
