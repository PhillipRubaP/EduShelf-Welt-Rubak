import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import api from '../services/api';
import './ConfirmEmail.css';

const ConfirmEmail = () => {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState('verifying'); // verifying, success, error
    const [message, setMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        const confirmEmail = async () => {
            const token = searchParams.get('token');
            const email = searchParams.get('email');

            if (!token || !email) {
                setStatus('error');
                setMessage('Invalid confirmation link.');
                return;
            }

            try {
                await api.post('/Users/confirm-email', { email, token });
                setStatus('success');
                setMessage('Email confirmed successfully! You can now log in.');
                setTimeout(() => {
                    navigate('/login');
                }, 3000);
            } catch (error) {
                console.error('Confirmation error:', error);
                setStatus('error');
                setMessage(error.response?.data?.message || 'Failed to confirm email. The link may have expired.');
            }
        };

        confirmEmail();
    }, [searchParams, navigate]);

    return (
        <div className="confirm-email-container">
            <div className="confirm-email-card">
                <h2>Email Confirmation</h2>
                {status === 'verifying' && <p className="status-verifying">Verifying your email...</p>}
                {status === 'success' && (
                    <div className="status-success">
                        <p>{message}</p>
                        <p>Redirecting to login...</p>
                        <Link to="/login" className="btn-login-link">Go to Login Now</Link>
                    </div>
                )}
                {status === 'error' && (
                    <div className="status-error">
                        <p>{message}</p>
                        <Link to="/login" className="btn-back">Back to Login</Link>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ConfirmEmail;
