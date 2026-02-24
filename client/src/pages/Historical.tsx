import React, { useState, useEffect } from 'react';
import Select from 'react-select';
import api from '../api/ApiClient';
import { FaCalendarAlt, FaAngleLeft, FaAngleRight } from 'react-icons/fa';

export default function Historical() {
    const [baseCurrency, setBaseCurrency] = useState({ value: 'USD', label: 'USD' });
    const [currencies, setCurrencies] = useState<{ value: string, label: string }[]>([]);
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
                const currencyOptions = ['USD', ...Object.keys(response.data.rates)].map(c => ({ value: c, label: c }));
                setCurrencies(currencyOptions);
            } catch (e) {
                console.error('Failed to fetch currencies', e);
            }
        };
        fetchCurrencies();
    }, []);

    const getHistoricalRates = async (newPage = page) => {
        setError(null);
        try {
            const response = await api.get(`/currency/historical?baseCurrency=${baseCurrency.value}&from=${fromDate}&to=${toDate}&page=${newPage}&pageSize=${pageSize}`);
            setHistoricalRates(response.data.items);
            setTotal(response.data.total);
        } catch (e: any) {
            setError(e?.response?.data?.error || e.message);
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
                <Select options={currencies} value={baseCurrency} onChange={(selected) => setBaseCurrency(selected as any)} />
                <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
                <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
                <button onClick={() => getHistoricalRates(1)}><FaCalendarAlt /> Get Historical Rates</button>
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
                        <button onClick={handlePrevPage} disabled={page === 1}><FaAngleLeft /> Previous</button>
                        <span style={{ margin: '0 1rem' }}>Page {page} of {Math.ceil(total / pageSize)}</span>
                        <button onClick={handleNextPage} disabled={page * pageSize >= total}><FaAngleRight /> Next</button>
                    </div>
                </div>
            )}
        </div>
    );
}
