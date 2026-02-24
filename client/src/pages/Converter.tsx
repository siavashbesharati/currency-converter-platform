import React, { useState, useEffect } from 'react';
import Select from 'react-select';
import api from '../api/ApiClient';
import { FaExchangeAlt } from 'react-icons/fa';

export default function Converter() {
  const [amount, setAmount] = useState(1);
  const [source, setSource] = useState({ value: 'USD', label: 'USD' });
  const [target, setTarget] = useState({ value: 'EUR', label: 'EUR' });
  const [result, setResult] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [currencies, setCurrencies] = useState<{ value: string, label: string }[]>([]);
  const [currenciesLoading, setCurrenciesLoading] = useState(false);
  const [converting, setConverting] = useState(false);

  useEffect(() => {
    const fetchCurrencies = async () => {
      try {
        setCurrenciesLoading(true);
        const response = await api.get('/currency/latest?baseCurrency=USD');
        const currencyOptions = ['USD', ...Object.keys(response.data.rates)].map(c => ({ value: c, label: c }));
        setCurrencies(currencyOptions);
      } catch (e) {
        console.error('Failed to fetch currencies', e);
      } finally {
        setCurrenciesLoading(false);
      }
    };
    fetchCurrencies();
  }, []);

  // Latest rates UI moved to its own page.

  const submit = async () => {
    setError(null);
    try {
      setConverting(true);
      const resp = await api.post('/currency/convert', { amount, source: source.value, target: target.value });
      setResult(resp.data.converted);
    } catch (e: any) {
      setError(e?.response?.data?.error || e.message);
    } finally {
      setConverting(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      submit();
    }
  };

  

  return (
    <div style={{ padding: 20 }}>
      <h2>Convert</h2>
      <div className="form-row">
        <input type="number" value={amount} onChange={e => setAmount(Number(e.target.value))} onKeyDown={handleKeyDown} />
        {currenciesLoading ? (
          <div>Loading currencies...</div>
        ) : (
          <>
            <Select options={currencies} value={source} onChange={(selected) => setSource(selected as any)} />
            <Select options={currencies} value={target} onChange={(selected) => setTarget(selected as any)} />
          </>
        )}
        <button className="btn-outline" onClick={submit} disabled={converting || currenciesLoading}>{converting ? 'Converting...' : (<><FaExchangeAlt /> Convert</>)}</button>
      </div>
      {result !== null && <h3>Converted Amount: {result.toFixed(4)} {target.label}</h3>}
      {error && <div className="error">Error: {error}</div>}
      
    </div>
  );
}
