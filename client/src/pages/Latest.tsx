import React, { useState } from 'react';
import Select from 'react-select';
import api from '../api/ApiClient';
import { FaRegChartBar, FaAngleLeft, FaAngleRight } from 'react-icons/fa';

export default function Latest() {
  const [base, setBase] = useState({ value: 'USD', label: 'USD' });
  const [currencies, setCurrencies] = useState<{ value: string, label: string }[]>([]);
  const [latestRates, setLatestRates] = useState<Record<string, number> | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  React.useEffect(() => {
    const fetch = async () => {
      try {
        const resp = await api.get('/currency/latest?baseCurrency=USD');
        const opts = ['USD', ...Object.keys(resp.data.rates)].map(c => ({ value: c, label: c }));
        setCurrencies(opts);
      } catch (e) {
        console.error(e);
      }
    };
    fetch();
  }, []);

  const getLatest = async (newPage = 1) => {
    setError(null);
    try {
      setLoading(true);
      const resp = await api.get(`/currency/latest?baseCurrency=${base.value}`);
      setLatestRates(resp.data.rates);
      setPage(1);
    } catch (e: any) {
      setError(e?.response?.data?.error || e.message);
    } finally {
      setLoading(false);
    }
  };

  const entries = latestRates ? Object.entries(latestRates) : [];
  const paginated = entries.slice((page - 1) * pageSize, page * pageSize);

  return (
    <div style={{ padding: 20 }}>
      <h2>Latest Rates</h2>
      <div className="form-row">
        {currencies.length === 0 ? <div>Loading currencies...</div> : <Select options={currencies} value={base} onChange={(s) => setBase(s as any)} />}
        <button className="btn-outline" onClick={() => getLatest(1)} disabled={loading || currencies.length === 0}>{loading ? 'Loading...' : (<><FaRegChartBar/> Get Latest Rates</>)}</button>
      </div>
      {error && <div className="error">Error: {error}</div>}
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
              {paginated.map(([c, r]) => (
                <tr key={c}><td>{c}</td><td>{(r as number).toFixed(4)}</td></tr>
              ))}
            </tbody>
          </table>
          <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', marginTop: '1rem' }}>
            <button className="btn-outline" onClick={() => setPage(p => p - 1)} disabled={page === 1}><FaAngleLeft/> Previous</button>
            <span style={{ margin: '0 1rem' }}>Page {page} of {Math.ceil(entries.length / pageSize)}</span>
            <button className="btn-outline" onClick={() => setPage(p => p + 1)} disabled={page * pageSize >= entries.length}><FaAngleRight/> Next</button>
          </div>
        </>
      )}
    </div>
  );
}
