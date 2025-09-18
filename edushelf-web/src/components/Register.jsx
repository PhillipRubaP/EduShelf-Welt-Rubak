import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import InfoDialog from './InfoDialog';
import api from '../services/api';
import './Auth.css';

const Register = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      await api.post('/Users', { username, email, password });
      navigate('/login');
    } catch (error) {
      console.error('Error during registration:', error);
      setError('An unexpected error occurred. Please try again.');
    }
  };

  return (
    <div className="auth-container">
      <InfoDialog message={error} onClose={() => setError('')} />
      <div className="auth-box">
        <h1 className="text-3xl font-bold text-center">Register</h1>
        <form onSubmit={handleRegister} className="space-y-6">
          <div className="form-group">
            <label className="text-sm font-medium mb-1">Username: </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-3 py-2 rounded-md bg-gray-700 text-white border border-gray-600 focus:outline-none focus:ring-2 focus:ring-primary-green"
              required
            />
          </div>
          <div className="form-group">
            <label className="text-sm font-medium mb-1">Email: </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 rounded-md bg-gray-700 text-white border border-gray-600 focus:outline-none focus:ring-2 focus:ring-primary-green"
              required
            />
          </div>
          <div className="form-group">
            <label className="text-sm font-medium mb-1">Password: </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 rounded-md bg-gray-700 text-white border border-gray-600 focus:outline-none focus:ring-2 focus:ring-primary-green"
              required
            />
          </div>
          <button
            type="submit"
            className="w-full px-4 py-2 mt-4 font-bold rounded-md bg-primary-green text-white hover:bg-secondary-mint hover:text-primary-green transition-colors button-auth"
          >
            Register
          </button>
        </form>
        <p className="text-center text-sm">
          Already have an account?{' '}
          <Link to="/login" className="text-highlight-amber hover:underline">
            Login
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Register;