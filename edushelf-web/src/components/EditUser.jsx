import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

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
      const response = await fetch(`http://localhost:49152/api/Users/${loggedInUser.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ userId: loggedInUser.id, username, email }),
      });
      if (response.ok) {
        setLoggedInUser({ ...loggedInUser, username, email });
        navigate('/');
      } else {
        alert('Update failed');
      }
    } catch (error) {
      console.error('Error during update:', error);
    }
  };

  const handleDelete = async () => {
    try {
      const response = await fetch(`http://localhost:49152/api/Users/${loggedInUser.id}`, {
        method: 'DELETE',
      });
      if (response.ok) {
        setLoggedInUser(null);
        navigate('/login');
      } else {
        alert('Delete failed');
      }
    } catch (error) {
      console.error('Error during delete:', error);
    }
  };

  if (!loggedInUser) {
    return <p>Please log in to edit your profile.</p>;
  }

  return (
    <div className="flex justify-center items-center h-screen bg-gray-900 text-white">
      <div className="w-full max-w-md p-8 space-y-6 bg-gray-800 rounded-lg shadow-md">
        <h1 className="text-2xl font-bold text-center">Edit Profile</h1>
        <form onSubmit={handleUpdate} className="space-y-6">
          <div>
            <label className="block text-sm font-medium">Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="w-full px-3 py-2 mt-1 text-white bg-gray-700 border border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          <div>
            <label className="block text-sm font-medium">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 mt-1 text-white bg-gray-700 border border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            />
          </div>
          <button
            type="submit"
            className="w-full px-4 py-2 font-bold text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Update
          </button>
        </form>
        <button
          onClick={handleDelete}
          className="w-full px-4 py-2 mt-4 font-bold text-white bg-red-600 rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
        >
          Delete Account
        </button>
      </div>
    </div>
  );
};

export default EditUser;