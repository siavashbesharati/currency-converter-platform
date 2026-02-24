import axios from 'axios';
import { v4 as uuidv4 } from 'uuid';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE || 'http://localhost:5000/api/v1',
});

const TOKEN_KEY = 'jwt';

let correlationId = uuidv4();

api.interceptors.request.use(config => {
    const token = localStorage.getItem(TOKEN_KEY);
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    config.headers['X-Correlation-ID'] = correlationId;
    return config;
});

api.interceptors.response.use(response => {
    const newCorrelationId = response.headers['x-correlation-id'];
    if (newCorrelationId) {
        correlationId = newCorrelationId;
    }
    return response;
});

export const login = async (username, password) => {
    const response = await api.post('/auth/login', { username, password });
    const token = response.data.access_token;
    localStorage.setItem(TOKEN_KEY, token);
    return token;
};

export const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
};

export const isLoggedIn = () => {
    return localStorage.getItem(TOKEN_KEY) !== null;
};


export default api;
