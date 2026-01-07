import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import InfoDialog from './InfoDialog';
import api from '../services/api';
import './Auth.css';

const Login = ({ setLoggedInUser }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const user = await api.post('/Users/login', { email, password });
      if (user && user.userId) {
        try {
          const roles = await api.get(`/Users/${user.userId}/roles`);
          user.roles = roles.map(r => r.name);
        } catch (roleError) {
          console.log('Could not fetch roles:', roleError);
          user.roles = [];
        }
        localStorage.setItem('user', JSON.stringify(user));
        setLoggedInUser(user);
        navigate('/');
      } else {
        setError('Invalid email or password.');
      }
    } catch (error) {
      console.error('Error during login:', error);
      setError('An unexpected error occurred. Please try again.');
    }
  };

  return (
    <div className="auth-container">
      <InfoDialog message={error} onClose={() => setError('')} />
      <div className="auth-box">
        <h1 className="text-3xl font-bold text-center">Login</h1>
        <form onSubmit={handleLogin} className="space-y-6">
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
            <label className="text-sm font-medium mb-1">Password: </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <button
            type="submit"
            className="w-full button-auth"
          >
            Login
          </button>
        </form>
        <p className="text-center text-sm">
          Don't have an account?{' '}
          <Link to="/register">
            Register
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Login;