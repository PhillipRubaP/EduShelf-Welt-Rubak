import { useEffect, useState } from 'react';
import { useSearchParams, Link, useNavigate } from 'react-router-dom';
import api from '../services/api';
import './VerifyEmail.css';

const VerifyEmail = () => {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState('verifying'); // verifying, success, error
    const [message, setMessage] = useState('Verifying your email...');
    const navigate = useNavigate();

    useEffect(() => {
        const verify = async () => {
            const email = searchParams.get('email');
            const token = searchParams.get('token');

            if (!email || !token) {
                setStatus('error');
                setMessage('Invalid verification link.');
                return;
            }

            try {
                await api.post('/Users/confirm-email', { email, token });
                setStatus('success');
                setMessage('Email verified successfully! You can now log in.');
                setTimeout(() => {
                    navigate('/login');
                }, 3000);
            } catch (error) {
                console.error('Verification failed', error);
                setStatus('error');
                setMessage('Verification failed. The link may be invalid or expired.');
            }
        };

        verify();
    }, [searchParams, navigate]);

    return (
        <div className="verify-container">
            <div className="verify-box">
                {status === 'verifying' && <h2>Verifying...</h2>}
                {status === 'success' && <h2 className="success-message">Success!</h2>}
                {status === 'error' && <h2 className="error-message">Error</h2>}

                <p>{message}</p>

                {status === 'success' && (
                    <Link to="/login" className="login-link">Go to Login</Link>
                )}

                {status === 'error' && (
                    <Link to="/login" className="login-link">Back to Login</Link>
                )}
            </div>
        </div>
    );
};

export default VerifyEmail;
