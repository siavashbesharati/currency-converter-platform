import React, { useState, useEffect } from 'react';
import Select from 'react-select';
import api from '../api/ApiClient';
import { FaCalendarAlt, FaAngleLeft, FaAngleRight } from 'react-icons/fa';

export default function Historical() {
    const [baseCurrency, setBaseCurrency] = useState({ value: 'USD', label: 'USD' });
    const [currencies, setCurrencies] = useState<{ value: string, label: string }[]>([]);
    const [currenciesLoading, setCurrenciesLoading] = useState(false);
    const [historicalLoading, setHistoricalLoading] = useState(false);
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

    const getHistoricalRates = async (newPage = page) => {
        setError(null);
        try {
            setHistoricalLoading(true);
            const response = await api.get(`/currency/historical?baseCurrency=${baseCurrency.value}&from=${fromDate}&to=${toDate}&page=${newPage}&pageSize=${pageSize}`);
            setHistoricalRates(response.data.items);
            setTotal(response.data.total);
        } catch (e: any) {
            setError(e?.response?.data?.error || e.message);
        } finally {
            setHistoricalLoading(false);
        }
    };
    
    const handleNextPage = () => {
        const newPage = page + 1;
        if ((newPage - 1) * pageSize < total) {
            setPage(newPage);
            getHistoricalRates(newPage);
        }
    };

    const handlePrevPage = () => {
        const newPage = page - 1;
        if (newPage > 0) {
            setPage(newPage);
            getHistoricalRates(newPage);
        }
    };

    return (
        <div style={{ padding: 20 }}>
            <h2>Historical Rates</h2>
            <div className="form-row">
                {currenciesLoading ? <div>Loading currencies...</div> : <Select options={currencies} value={baseCurrency} onChange={(selected) => setBaseCurrency(selected as any)} />}
                <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
                <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
                <button className="btn-outline" onClick={() => getHistoricalRates(1)} disabled={historicalLoading || currenciesLoading}>{historicalLoading ? 'Loading...' : (<><FaCalendarAlt /> Get Historical Rates</>)}</button>
            </div>
            {error && <div className="error">Error: {error}</div>}
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
                                                <li key={currency}>{currency}: {value.toFixed(4)}</li>
                                            ))}
                                        </ul>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', marginTop: '1rem' }}>
                        <button className="btn-outline" onClick={handlePrevPage} disabled={page === 1}><FaAngleLeft /> Previous</button>
                        <span style={{ margin: '0 1rem' }}>Page {page} of {Math.ceil(total / pageSize)}</span>
                        <button className="btn-outline" onClick={handleNextPage} disabled={page * pageSize >= total}><FaAngleRight /> Next</button>
                    </div>
                </div>
            )}
        </div>
    );
}
