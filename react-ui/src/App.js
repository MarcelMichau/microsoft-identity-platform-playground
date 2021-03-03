import React from 'react';
import {
	useAccount,
	useMsalAuthentication,
	useMsal,
	AuthenticatedTemplate,
	UnauthenticatedTemplate,
} from '@azure/msal-react';
import Weather from './Weather';

function App() {
	const { login, result, error } = useMsalAuthentication('popup');
	const { instance, accounts } = useMsal();

	const account = useAccount(accounts[0] || {});

	return (
		<>
			<p>Anyone can see this paragraph.</p>
			<AuthenticatedTemplate>
				<p>There are currently: {accounts.length} account(s) signed in.</p>

				<div>
					<p>Signed in user:</p>
					<pre>{JSON.stringify(account, null, '\t')}</pre>
				</div>

				<button onClick={() => instance.logout()}>Log Out</button>

				<Weather />
			</AuthenticatedTemplate>
			<UnauthenticatedTemplate>
				<p>No users are signed in!</p>
			</UnauthenticatedTemplate>
		</>
	);
}

export default App;
