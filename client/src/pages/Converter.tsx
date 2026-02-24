import React, { useState, useEffect } from 'react';
import Select from 'react-select';
import api from '../api/ApiClient';
import { FaExchangeAlt, FaRegChartBar, FaAngleLeft, FaAngleRight } from 'react-icons/fa';

export default function Converter() {
  const [amount, setAmount] = useState(1);
  const [source, setSource] = useState({ value: 'USD', label: 'USD' });
  const [target, setTarget] = useState({ value: 'EUR', label: 'EUR' });
  const [result, setResult] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [currencies, setCurrencies] = useState<{ value: string, label: string }[]>([]);
  const [latestRates, setLatestRates] = useState<Record<string, number> | null>(null);
  const [latestRatesPage, setLatestRatesPage] = useState(1);
  const latestRatesPageSize = 10;

  useEffect(() => {
    const fetchCurrencies = async () => {
      try {
        const response = await api.get('/currency/latest?baseCurrency=USD');
        const currencyOptions = ['USD', ...Object.keys(response.data.rates)].map(c => ({ value: c, label: c }));
        setCurrencies(currencyOptions);
      } catch (e) {
        console.error('Failed to fetch currencies', e);
      }
    };
    fetchCurrencies();
  }, []);

  const handleGetLatest = async () => {
    setError(null);
    try {
        const response = await api.get(`/currency/latest?baseCurrency=${source.value}`);
        setLatestRates(response.data.rates);
        setLatestRatesPage(1); // Reset to first page
    } catch (e: any) {
        setError(e?.response?.data?.error || e.message);
    }
  };

  const submit = async () => {
    setError(null);
    try {
      const resp = await api.post('/currency/convert', { amount, source: source.value, target: target.value });
      setResult(resp.data.converted);
    } catch (e: any) {
      setError(e?.response?.data?.error || e.message);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      submit();
    }
  };

  const latestRatesEntries = latestRates ? Object.entries(latestRates) : [];
  const paginatedLatestRates = latestRatesEntries.slice((latestRatesPage - 1) * latestRatesPageSize, latestRatesPage * latestRatesPageSize);
  const totalLatestRatesPages = Math.ceil(latestRatesEntries.length / latestRatesPageSize);

  return (
    <div style={{ padding: 20 }}>
      <h2>Convert</h2>
      <div className="form-row">
        <input type="number" value={amount} onChange={e => setAmount(Number(e.target.value))} onKeyDown={handleKeyDown} />
        <Select options={currencies} value={source} onChange={(selected) => setSource(selected as any)} />
        <Select options={currencies} value={target} onChange={(selected) => setTarget(selected as any)} />
        <button onClick={submit}><FaExchangeAlt /> Convert</button>
      </div>
      {result !== null && <h3>Converted Amount: {result.toFixed(4)} {target.label}</h3>}
      {error && <div className="error">Error: {error}</div>}

      <hr style={{margin: '2rem 0'}}/>

      <h2>Latest Rates</h2>
      <div className="form-row">
        <Select options={currencies} value={source} onChange={(selected) => setSource(selected as any)} />
        <button onClick={handleGetLatest}><FaRegChartBar /> Get Latest Rates</button>
      </div>
      {latestRates && (
        <>
            <table>
              <thead>
                <tr>
                  <th>Currency</th>
                  <th>Rate</th>
                </tr>
              </thead>
              <tbody>
                {paginatedLatestRates.map(([currency, rate]) => (
                  <tr key={currency}>
                    <td>{currency}</td>
                    <td>{rate.toFixed(4)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', marginTop: '1rem' }}>
                <button onClick={() => setLatestRatesPage(p => p - 1)} disabled={latestRatesPage === 1}><FaAngleLeft/> Previous</button>
                <span style={{ margin: '0 1rem' }}>Page {latestRatesPage} of {totalLatestRatesPages}</span>
                <button onClick={() => setLatestRatesPage(p => p + 1)} disabled={latestRatesPage === totalLatestRatesPages}><FaAngleRight/> Next</button>
            </div>
        </>
      )}
    </div>
  );
}
