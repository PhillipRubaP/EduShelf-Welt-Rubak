import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import './EditUser.css';
import InfoDialog from './InfoDialog';

const EditUser = ({ loggedInUser, setLoggedInUser }) => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPasswordFields, setShowPasswordFields] = useState(false);
  const [info, setInfo] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    if (loggedInUser) {
      setUsername(loggedInUser.username);
      setEmail(loggedInUser.email);
    }
  }, [loggedInUser]);

  const handleUpdate = async (e) => {
    e.preventDefault();

    if (showPasswordFields) {
      if (password !== confirmPassword) {
        setInfo("Passwords do not match!");
        return;
      }
      if (password.length > 0 && password.length < 6) {
        setInfo("Password must be at least 6 characters long.");
        return;
      }
    }

    try {
      const updateData = { userId: loggedInUser.id, username, email };
      if (showPasswordFields && password) {
        updateData.password = password;
      }

      await api.put(`/Users/${loggedInUser.id}`, updateData);
      setLoggedInUser({ ...loggedInUser, username, email });
      setInfo("Profile updated successfully!");

      // Navigate after a short delay to let user see success message
      setTimeout(() => {
        navigate('/');
      }, 1500);

    } catch (error) {
      console.error('Error during update:', error);
      setInfo("Failed to update profile. Please try again.");
    }
  };

  const handleDelete = async () => {
    if (window.confirm("Are you sure you want to delete your account? This action cannot be undone.")) {
      try {
        await api.delete('/Users/me');
        setLoggedInUser(null);
        navigate('/login');
      } catch (error) {
        console.error('Error during delete:', error);
        setInfo("Failed to delete account.");
      }
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
      {info && <InfoDialog message={info} onClose={() => setInfo('')} />}
      <div className="auth-box">
        <div className="modal-close-button" onClick={handleClose}>
          <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>

        <div className="auth-header">
          <h1>Edit Profile</h1>
        </div>

        <form onSubmit={handleUpdate}>
          <div className="form-section">
            <div className="section-title">Profile Info</div>
            <div className="form-group">
              <label>Username</label>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="form-input"
                required
              />
            </div>
            <div className="form-group">
              <label>Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="form-input"
                required
              />
            </div>
          </div>

          <div className="form-section">
            <div className="security-header">
              <button
                type="button"
                onClick={() => setShowPasswordFields(!showPasswordFields)}
                className="toggle-password-btn"
              >
                {showPasswordFields ? 'Cancel Password Change' : 'Change Password'}
              </button>
            </div>

            {showPasswordFields && (
              <div className="animate-fade-in-down">
                <div className="form-group">
                  <label>New Password</label>
                  <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="form-input"
                    placeholder="Leave empty to keep current"
                  />
                </div>
                <div className="form-group">
                  <label>Confirm Password</label>
                  <input
                    type="password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    className="form-input"
                    placeholder="Confirm new password"
                  />
                </div>
              </div>
            )}
          </div>

          <div className="button-container">
            <button type="submit" className="btn btn-primary">
              Save Changes
            </button>
            <button
              type="button"
              onClick={handleDelete}
              className="btn btn-danger"
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