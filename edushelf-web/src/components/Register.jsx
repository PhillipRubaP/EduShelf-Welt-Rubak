import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import InfoDialog from './InfoDialog';
import api from '../services/api';
import './Auth.css';

const Register = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [info, setInfo] = useState('');
  const navigate = useNavigate();

  const validate = () => {
    const newErrors = {};
    if (!username) {
      newErrors.username = 'Username is required.';
    } else if (username.length < 3 || username.length > 20) {
      newErrors.username = 'Username must be between 3 and 20 characters.';
    }

    if (!email) {
      newErrors.email = 'Email is required.';
    } else if (!/\S+@\S+\.\S+/.test(email)) {
      newErrors.email = 'A valid email address is required.';
    }

    if (!password) {
      newErrors.password = 'Password is required.';
    } else if (password.length < 6) {
      newErrors.password = 'Password must be at least 6 characters long.';
    }

    setErrors(newErrors);
    return newErrors;
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    const validationErrors = validate();
    
    if (Object.keys(validationErrors).length > 0) {
      const errorMessages = Object.values(validationErrors).join('\n');
      setInfo(errorMessages);
      return;
    }

    try {
      await api.post('/Users', { username, email, password });
      navigate('/login');
    } catch (error) {
      console.error('Error during registration:', error);
      if (error.response && error.response.data && error.response.data.errors) {
        const backendErrors = {};
        const errorMessages = [];
        for (const key in error.response.data.errors) {
          const formattedKey = key.charAt(0).toLowerCase() + key.slice(1);
          backendErrors[formattedKey] = error.response.data.errors[key].join(' ');
          errorMessages.push(backendErrors[formattedKey]);
        }
        setErrors(backendErrors);
        setInfo(errorMessages.join('\n'));
      } else {
        setInfo('An unexpected error occurred. Please try again.');
      }
    }
  };

  return (
    <div className="auth-container">
      <InfoDialog message={info} onClose={() => setInfo('')} />
      <div className="auth-box">
        <h1 className="text-3xl font-bold text-center">Register</h1>
        <form onSubmit={handleRegister} className="space-y-4" noValidate>
          <div className="form-group">
            <label className="text-sm font-medium mb-1">Username: </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className={errors.username ? 'error' : ''}
            />
          </div>
          <div className="form-group">
            <label className="text-sm font-medium mb-1">Email: </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={errors.email ? 'error' : ''}
            />
          </div>
          <div className="form-group">
            <label className="text-sm font-medium mb-1">Password: </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className={errors.password ? 'error' : ''}
            />
          </div>
          <button
            type="submit"
            className="w-full button-auth"
          >
            Register
          </button>
        </form>
        <p className="text-center text-sm mt-4">
          Already have an account?{' '}
          <Link to="/login">
            Login
          </Link>
        </p>
      </div>
    </div>
  );
};

export default Register;