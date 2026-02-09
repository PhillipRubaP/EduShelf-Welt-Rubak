import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import InfoDialog from './InfoDialog';
import api from '../services/api';
import './Auth.css';

const EmailConfirmation = () => {
    const [searchParams] = useSearchParams();
    const [email, setEmail] = useState('');
    const [token, setToken] = useState('');
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const emailParam = searchParams.get('email');
        const tokenParam = searchParams.get('token');
        if (emailParam) setEmail(emailParam);
        if (tokenParam) setToken(tokenParam);
    }, [searchParams]);

    const handleConfirm = async (e) => {
        e.preventDefault();
        try {
            await api.post('/Users/confirm-email', { email, token });
            setMessage('Email confirmed successfully! You can now login.');
            setTimeout(() => {
                navigate('/login');
            }, 3000);
        } catch (error) {
            console.error('Error confirming email:', error);
            setError('Invalid email or token. Please try again.');
        }
    };

    return (
        <div className="auth-container">
            <InfoDialog message={message || error} onClose={() => {
                setMessage('');
                setError('');
            }} />
            <div className="auth-box">
                <h1 className="text-3xl font-bold text-center">Confirm Email</h1>
                <p className="text-center text-sm mb-4">
                    Please enter your email and the confirmation token sent to you.
                </p>
                <form onSubmit={handleConfirm} className="space-y-6">
                    <div className="form-group">
                        <label className="text-sm font-medium mb-1">Email: </label>
                        <input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                        />
                    </div>
                    <div className="form-group">
                        <label className="text-sm font-medium mb-1">Token: </label>
                        <input
                            type="text"
                            value={token}
                            onChange={(e) => setToken(e.target.value)}
                            required
                        />
                    </div>
                    <button
                        type="submit"
                        className="w-full button-auth"
                    >
                        Confirm Email
                    </button>
                </form>
            </div>
        </div>
    );
};

export default EmailConfirmation;
