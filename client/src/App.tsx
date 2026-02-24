import React, { useState } from 'react';
import { login, logout, isLoggedIn } from './api/ApiClient';
import Converter from './pages/Converter';
import Historical from './pages/Historical';
import Latest from './pages/Latest';
import { FaSignOutAlt, FaSignInAlt, FaExchangeAlt, FaRegChartBar, FaCalendarAlt } from 'react-icons/fa';

export default function App() {
  const [loggedIn, setLoggedIn] = useState(isLoggedIn());
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loginLoading, setLoginLoading] = useState(false);
  const [page, setPage] = useState('converter');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoginLoading(true);
      await login(username, password);
      setLoggedIn(true);
    } catch (error) {
      console.error('Login failed', error);
      alert('Login failed. Please check the console for details.');
    } finally {
      setLoginLoading(false);
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
          <button type="submit" className="btn-outline" disabled={loginLoading}>{loginLoading ? 'Logging in...' : (<><FaSignInAlt/> Login</>)}</button>
        </form>
      </div>
    );
  }

  return (
    <div>
        <div style={{display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem'}}>
            <h1>Currency Converter</h1>
                <button className="btn-outline" onClick={handleLogout}><FaSignOutAlt/> Logout</button>
        </div>
      
          <div className="menu" style={{marginBottom: '2rem'}}>
            <button className="btn-outline" onClick={() => setPage('converter')} disabled={page === 'converter'}><FaExchangeAlt/> 🔁 Converter</button>
            <button className="btn-outline" onClick={() => setPage('latest')} disabled={page === 'latest'}><FaRegChartBar/> 📈 Latest</button>
            <button className="btn-outline" onClick={() => setPage('historical')} disabled={page === 'historical'}><FaCalendarAlt/> 🕰️ Historical</button>
          </div>

          {page === 'converter' && <Converter />}
          {page === 'latest' && <Latest />}
          {page === 'historical' && <Historical />}
    </div>
  );
}
