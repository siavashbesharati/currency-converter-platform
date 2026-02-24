import React, { useState } from 'react';
import { login, logout, isLoggedIn } from './api/ApiClient';
import Converter from './pages/Converter';
import Historical from './pages/Historical';
import { FaSignOutAlt, FaSignInAlt } from 'react-icons/fa';

export default function App() {
  const [loggedIn, setLoggedIn] = useState(isLoggedIn());
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [page, setPage] = useState('converter');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await login(username, password);
      setLoggedIn(true);
    } catch (error) {
      console.error('Login failed', error);
      alert('Login failed. Please check the console for details.');
    }
  };

  const handleLogout = () => {
    logout();
    setLoggedIn(false);
  };

  if (!loggedIn) {
    return (
      <div style={{ maxWidth: '400px', margin: 'auto' }}>
        <h1>Login</h1>
        <p>Use username: <strong>demo</strong> and password: <strong>demo</strong></p>
        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label>Username</label>
            <input type="text" value={username} onChange={e => setUsername(e.target.value)} />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input type="password" value={password} onChange={e => setPassword(e.target.value)} />
          </div>
          <button type="submit"><FaSignInAlt/> Login</button>
        </form>
      </div>
    );
  }

  return (
    <div>
        <div style={{display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem'}}>
            <h1>Currency Converter</h1>
            <button onClick={handleLogout}><FaSignOutAlt/> Logout</button>
        </div>
      
      <div style={{marginBottom: '2rem'}}>
        <button onClick={() => setPage('converter')} disabled={page === 'converter'}>Converter</button>
        <button onClick={() => setPage('historical')} disabled={page === 'historical'}>Historical</button>
      </div>
      
      {page === 'converter' ? <Converter /> : <Historical />}
    </div>
  );
}
