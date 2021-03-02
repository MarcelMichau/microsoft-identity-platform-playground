import React, { useState } from 'react';
import { useMsal, useAccount } from '@azure/msal-react';

const Weather = () => {
	const { instance, accounts } = useMsal();
	const account = useAccount(accounts[0]);
	const [weatherInfo, setWeatherInfo] = useState([]);

	const fetchBasicWeather = async () => {
		const apiUrl = 'https://localhost:5001/WeatherForecast';

		if (account) {
			const response = await instance.acquireTokenPopup({
				scopes: [
					'api://73e9f49d-c747-4443-b973-74288ac6e6d8/Weather.Read.Basic',
				],
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

	const fetchLotsOfWeather = async () => {
		const apiUrl = 'https://localhost:5001/WeatherForecast/lots';

		if (account) {
			const response = await instance.acquireTokenPopup({
				scopes: [
					'api://73e9f49d-c747-4443-b973-74288ac6e6d8/Weather.Read.Lots',
				],
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

	const fetchSpecialWeather = async () => {
		const apiUrl = 'https://localhost:5001/WeatherForecast/special';

		if (account) {
			const response = await instance.acquireTokenPopup({
				scopes: [
					'api://73e9f49d-c747-4443-b973-74288ac6e6d8/Weather.Read.Special',
				],
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
			<button onClick={() => fetchBasicWeather()}>
				Get Basic Weather From API
			</button>

			<button onClick={() => fetchLotsOfWeather()}>
				Get Lots of Weather From API
			</button>

			<button onClick={() => fetchSpecialWeather()}>
				Get Special Weather From API
			</button>

			<pre>{JSON.stringify(weatherInfo, null, '\t')}</pre>
		</div>
	);
};
export default Weather;
