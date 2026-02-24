import React, { useState, useEffect } from 'react';
import api from '../api/ApiClient';

export default function Converter() {
  const [amount, setAmount] = useState(1);
  const [source, setSource] = useState('USD');
  const [target, setTarget] = useState('EUR');
  const [result, setResult] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [currencies, setCurrencies] = useState<string[]>([]);
  const [latestRates, setLatestRates] = useState<Record<string, decimal> | null>(null);

  useEffect(() => {
    const fetchCurrencies = async () => {
      try {
        const response = await api.get('/currency/latest?baseCurrency=USD');
        setCurrencies(['USD', ...Object.keys(response.data.rates)]);
      } catch (e) {
        console.error('Failed to fetch currencies', e);
      }
    };
    fetchCurrencies();
  }, []);

  const handleGetLatest = async () => {
    setError(null);
    try {
        const response = await api.get(`/currency/latest?baseCurrency=${source}`);
        setLatestRates(response.data.rates);
    } catch (e: any) {
        setError(e?.response?.data?.error || e.message);
    }
  };

  const submit = async () => {
    setError(null);
    try {
      const resp = await api.post('/currency/convert', { amount, source, target });
      setResult(resp.data.converted);
    } catch (e: any) {
      setError(e?.response?.data?.error || e.message);
    }
  };

  return (
    <div style={{ padding: 20 }}>
      <h2>Convert</h2>
      <div>
        <input type="number" value={amount} onChange={e => setAmount(Number(e.target.value))} />
        <select value={source} onChange={e => setSource(e.target.value)}>
          {currencies.map(c => <option key={c} value={c}>{c}</option>)}
        </select>
        <select value={target} onChange={e => setTarget(e.target.value)}>
          {currencies.map(c => <option key={c} value={c}>{c}</option>)}
        </select>
        <button onClick={submit}>Convert</button>
      </div>
      {result !== null && <div>Converted: {result}</div>}
      {error && <div style={{ color: 'red' }}>Error: {error}</div>}

      <hr />

      <h2>Latest Rates</h2>
      <div>
        <select value={source} onChange={e => setSource(e.target.value)}>
            {currencies.map(c => <option key={c} value={c}>{c}</option>)}
        </select>
        <button onClick={handleGetLatest}>Get Latest Rates</button>
      </div>
      {latestRates && (
        <ul>
          {Object.entries(latestRates).map(([currency, rate]) => (
            <li key={currency}>{currency}: {rate}</li>
          ))}
        </ul>
      )}
    </div>
  );
}
