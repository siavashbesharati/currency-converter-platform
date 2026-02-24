import React, { useState, useEffect } from 'react';
import api from '../api/ApiClient';

export default function Historical() {
    const [baseCurrency, setBaseCurrency] = useState('USD');
    const [currencies, setCurrencies] = useState<string[]>([]);
    const [fromDate, setFromDate] = useState('');
    const [toDate, setToDate] = useState('');
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [historicalRates, setHistoricalRates] = useState<any[]>([]);
    const [total, setTotal] = useState(0);
    const [error, setError] = useState<string | null>(null);

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

    const getHistoricalRates = async () => {
        setError(null);
        try {
            const response = await api.get(`/currency/historical?baseCurrency=${baseCurrency}&from=${fromDate}&to=${toDate}&page=${page}&pageSize=${pageSize}`);
            setHistoricalRates(response.data.items);
            setTotal(response.data.total);
        } catch (e: any) {
            setError(e?.response?.data?.error || e.message);
        }
    };

    const handleNextPage = () => {
        if (page * pageSize < total) {
            setPage(page + 1);
            getHistoricalRates();
        }
    };

    const handlePrevPage = () => {
        if (page > 1) {
            setPage(page - 1);
            getHistoricalRates();
        }
    };


    return (
        <div style={{ padding: 20 }}>
            <h2>Historical Rates</h2>
            <div>
                <select value={baseCurrency} onChange={e => setBaseCurrency(e.target.value)}>
                    {currencies.map(c => <option key={c} value={c}>{c}</option>)}
                </select>
                <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
                <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
                <button onClick={getHistoricalRates}>Get Historical Rates</button>
            </div>
            {error && <div style={{ color: 'red' }}>Error: {error}</div>}
            {historicalRates.length > 0 && (
                <div>
                    <table>
                        <thead>
                            <tr>
                                <th>Date</th>
                                <th>Rates</th>
                            </tr>
                        </thead>
                        <tbody>
                            {historicalRates.map((rate: any) => (
                                <tr key={rate.date}>
                                    <td>{rate.date}</td>
                                    <td>
                                        <ul>
                                            {Object.entries(rate.rates).map(([currency, value] : [string, any]) => (
                                                <li key={currency}>{currency}: {value}</li>
                                            ))}
                                        </ul>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <div>
                        <button onClick={handlePrevPage} disabled={page === 1}>Previous</button>
                        <span>Page {page} of {Math.ceil(total / pageSize)}</span>
                        <button onClick={handleNextPage} disabled={page * pageSize >= total}>Next</button>
                    </div>
                </div>
            )}
        </div>
    );
}
