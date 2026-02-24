import React, { useState, useEffect } from 'react';
import { login, logout, isLoggedIn } from './api/ApiClient';
import Converter from './pages/Converter';

import Historical from './pages/Historical';

export default function App() {
  const [loggedIn, setLoggedIn] = useState(isLoggedIn());
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [page, setPage] = useState('converter');

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      await login(username, password);
      setLoggedIn(true);
    } catch (error) {
      console.error('Login failed', error);
    }
  };

  const handleLogout = () => {
    logout();
    setLoggedIn(false);
  };

  if (!loggedIn) {
    return (
      <div style={{ padding: 20 }}>
        <h1>Login</h1>
        <form onSubmit={handleLogin}>
          <div>
            <label>Username</label>
            <input type="text" value={username} onChange={e => setUsername(e.target.value)} />
          </div>
          <div>
            <label>Password</label>
            <input type="password" value={password} onChange={e => setPassword(e.target.value)} />
          </div>
          <button type="submit">Login</button>
        </form>
      </div>
    );
  }

  return (
    <div style={{ padding: 20 }}>
      <h1>Currency Converter</h1>
      <button onClick={handleLogout}>Logout</button>
      <div>
        <button onClick={() => setPage('converter')}>Converter</button>
        <button onClick={() => setPage('historical')}>Historical</button>
      </div>
      {page === 'converter' ? <Converter /> : <Historical />}
    </div>
  );
}
