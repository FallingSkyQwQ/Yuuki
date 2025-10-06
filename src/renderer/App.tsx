import React, { useState, useEffect } from 'react';
import './App.css';

interface AccountData {
  id: string;
  username: string;
  type: string;
  uuid: string;
}

interface ProfileData {
  id: string;
  name: string;
  version: string;
  mods?: unknown[];
}

interface GameStatusData {
  status: string;
}

const App: React.FC = () => {
  const [appVersion, setAppVersion] = useState<string>('Loading...');
  const [accounts, setAccounts] = useState<AccountData[]>([]);
  const [profiles, setProfiles] = useState<ProfileData[]>([]);
  const [gameStatus, setGameStatus] = useState<GameStatusData | null>(null);

  useEffect(() => {
    // Load initial data
    loadAppData();
  }, []);

  const loadAppData = async () => {
    try {
      // Get app version
      const version = await window.electronAPI.getAppVersion();
      setAppVersion(version);

      // Get accounts
      const accountList = await window.electronAPI.getAccounts() as AccountData[];
      setAccounts(accountList);

      // Get profiles
      const profileList = await window.electronAPI.getProfiles() as ProfileData[];
      setProfiles(profileList);

      // Get game status
      const status = await window.electronAPI.getGameStatus() as GameStatusData;
      setGameStatus(status);
    } catch (error) {
      // TODO: Implement proper error handling UI
      console.error('Failed to load app data:', error);
    }
  };

  const handleCreateAccount = async () => {
    try {
      const newAccount = await window.electronAPI.createAccount({
        type: 'offline',
        username: 'TestUser'
      });
      setAccounts([...accounts, newAccount]);
    } catch (error) {
      // TODO: Implement proper error handling UI
      console.error('Failed to create account:', error);
    }
  };

  const handleCreateProfile = async () => {
    try {
      const newProfile = await window.electronAPI.createProfile({
        name: 'Test Profile',
        version: '1.20.1'
      });
      setProfiles([...profiles, newProfile]);
    } catch (error) {
      // TODO: Implement proper error handling UI
      console.error('Failed to create profile:', error);
    }
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>Yuuki Minecraft Launcher</h1>
        <p className="app-version">Version: {appVersion}</p>
      </header>

      <main className="app-main">
        <div className="section">
          <h2>Accounts ({accounts.length})</h2>
          <button onClick={handleCreateAccount} className="btn btn-primary">
            Create Test Account
          </button>
          <div className="account-list">
            {accounts.map((account: AccountData) => (
              <div key={account.id} className="account-card">
                <h3>{account.username}</h3>
                <p>Type: {account.type}</p>
                <p>UUID: {account.uuid}</p>
              </div>
            ))}
          </div>
        </div>

        <div className="section">
          <h2>Game Profiles ({profiles.length})</h2>
          <button onClick={handleCreateProfile} className="btn btn-primary">
            Create Test Profile
          </button>
          <div className="profile-list">
            {profiles.map((profile: ProfileData) => (
              <div key={profile.id} className="profile-card">
                <h3>{profile.name}</h3>
                <p>Version: {profile.version}</p>
                <p>Mods: {profile.mods?.length || 0}</p>
              </div>
            ))}
          </div>
        </div>

        <div className="section">
          <h2>System Info</h2>
          <div className="system-info">
            <p>Platform: {window.electronAPI.platform}</p>
            <p>Architecture: {window.electronAPI.arch}</p>
            <p>Game Status: {(gameStatus as GameStatusData)?.status || 'Unknown'}</p>
          </div>
        </div>
      </main>

      <footer className="app-footer">
        <p>Ready to launch Minecraft adventures!</p>
      </footer>
    </div>
  );
};

export default App;