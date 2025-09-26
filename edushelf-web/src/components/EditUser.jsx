import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import './EditUser.css';

const EditUser = ({ loggedInUser, setLoggedInUser }) => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (loggedInUser) {
      setUsername(loggedInUser.username);
      setEmail(loggedInUser.email);
    }
  }, [loggedInUser]);

  const handleUpdate = async (e) => {
    e.preventDefault();
    try {
      await api.put(`/Users/${loggedInUser.id}`, { userId: loggedInUser.id, username, email });
      setLoggedInUser({ ...loggedInUser, username, email });
      navigate('/');
    } catch (error) {
      console.error('Error during update:', error);
    }
  };

  const handleDelete = async () => {
    try {
      await api.delete(`/Users/${loggedInUser.id}`);
      setLoggedInUser(null);
      navigate('/login');
    } catch (error) {
      console.error('Error during delete:', error);
    }
  };

  const handleClose = () => {
    navigate('/');
  };

  if (!loggedInUser) {
    return <p>Please log in to edit your profile.</p>;
  }

  return (
    <div className="edit-user-container">
      <div className="auth-box">
        <span className="close-button" onClick={handleClose}>&times;</span>
        <h1 className="text-3xl font-bold text-center">Edit Profile</h1>
        <form onSubmit={handleUpdate} className="space-y-6">
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
          <div className="button-container">
            <button
              type="submit"
              className="w-full px-4 py-2 mt-4 font-bold rounded-md bg-primary-green text-white hover:bg-secondary-mint hover:text-primary-green transition-colors button-auth"
            >
              Update
            </button>
            <button
              onClick={handleDelete}
              className="w-full px-4 py-2 mt-4 font-bold rounded-md bg-red-600 text-white hover:bg-red-700 transition-colors button-auth"
            >
              Delete Account
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditUser;