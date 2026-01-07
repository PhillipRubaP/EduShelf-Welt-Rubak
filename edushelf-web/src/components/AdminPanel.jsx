
import React, { useState, useEffect } from 'react';
import api from '../services/api';
import './AdminPanel.css';

const AdminPanel = () => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const [userFiles, setUserFiles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [editUserMode, setEditUserMode] = useState(false);
    const [editUserData, setEditUserData] = useState({ username: '', email: '' });
    const [editFileId, setEditFileId] = useState(null);
    const [editFileTitle, setEditFileTitle] = useState('');

    useEffect(() => {
        fetchUsers();
    }, []);

    const fetchUsers = async () => {
        try {
            setLoading(true);
            const data = await api.get('/Users');
            setUsers(data);
        } catch (err) {
            console.error('Failed to fetch users:', err);
            setError('Could not load users.');
        } finally {
            setLoading(false);
        }
    };

    const fetchUserFiles = async (userId) => {
        try {
            setLoading(true);
            const files = await api.get(`/Users/${userId}/documents`);
            setUserFiles(files);
        } catch (err) {
            console.error('Failed to fetch user files:', err);
            setError('Could not load user files.');
        } finally {
            setLoading(false);
        }
    };

    const handleUserClick = (user) => {
        setSelectedUser(user);
        setError('');
        setEditUserMode(false);
        fetchUserFiles(user.userId);
    };

    const handleBackToUsers = () => {
        setSelectedUser(null);
        setUserFiles([]);
        setError('');
    };

    const handleDeleteUser = async (userId) => {
        if (!window.confirm('Are you sure you want to delete this user? This cannot be undone.')) return;
        try {
            await api.delete(`/Users/${userId}`);
            setUsers(users.filter(u => u.userId !== userId));
            if (selectedUser && selectedUser.userId === userId) {
                handleBackToUsers();
            }
        } catch (err) {
            console.error('Failed to delete user:', err);
            setError('Failed to delete user.');
        }
    };

    const handleEditUserClick = (user) => {
        setEditUserMode(true);
        setEditUserData({ username: user.username, email: user.email });
    };

    const handleSaveUser = async () => {
        try {
            await api.patch(`/Users/${selectedUser.userId}`, editUserData);
            setUsers(users.map(u => (u.userId === selectedUser.userId ? { ...u, ...editUserData } : u)));
            setSelectedUser({ ...selectedUser, ...editUserData });
            setEditUserMode(false);
        } catch (err) {
            console.error('Failed to update user:', err);
            setError('Failed to update user.');
        }
    };

    const handleDeleteFile = async (fileId) => {
        if (!window.confirm('Are you sure you want to delete this file?')) return;
        try {
            await api.delete(`/Documents/${fileId}`);
            setUserFiles(userFiles.filter(f => f.id !== fileId));
        } catch (err) {
            console.error('Failed to delete file:', err);
            setError('Failed to delete file.');
        }
    };

    const startEditFile = (file) => {
        setEditFileId(file.id);
        setEditFileTitle(file.title);
    };

    const handleSaveFile = async (fileId) => {
        try {
            // Need to fetch the full document to update it properly with PUT, 
            // but the API requires the full object. 
            // Or we can try to just update what we have if the backend accepts partial or if we fetch first.
            // The PUT endpoint expects an Entity object, which might be tricky if we don't have all fields.
            // However, let's try to fetch, update title, and put back.

            // Step 1: Get single document details (including proper structure if needed)
            const doc = await api.get(`/Documents/${fileId}`);

            // Step 2: Update title
            const updatedDoc = { ...doc, title: editFileTitle };

            // Step 3: Put
            await api.put(`/Documents/${fileId}`, updatedDoc);

            setUserFiles(userFiles.map(f => (f.id === fileId ? { ...f, title: editFileTitle } : f)));
            setEditFileId(null);
        } catch (err) {
            console.error('Failed to update file:', err);
            setError('Failed to update file.');
        }
    };

    return (
        <div className="admin-panel-container">
            <h2 className="admin-title">Admin Panel</h2>
            {error && <div className="error-message">{error}</div>}

            {!selectedUser ? (
                <div className="users-list-section">
                    <h3>All Users</h3>
                    {loading ? <p>Loading users...</p> : (
                        <table className="users-table">
                            <thead>
                                <tr>
                                    <th>ID</th>
                                    <th>Username</th>
                                    <th>Email</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {users.map(user => (
                                    <tr key={user.userId}>
                                        <td>{user.userId}</td>
                                        <td className="clickable-cell" onClick={() => handleUserClick(user)}>{user.username}</td>
                                        <td className="clickable-cell" onClick={() => handleUserClick(user)}>{user.email}</td>
                                        <td>
                                            <button className="btn-icon delete" onClick={() => handleDeleteUser(user.userId)}>🗑️</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>
            ) : (
                <div className="user-details-section">
                    <button className="btn-back" onClick={handleBackToUsers}>← Back to Users</button>

                    <div className="user-info-card">
                        {editUserMode ? (
                            <div className="edit-user-form">
                                <h3>Edit User</h3>
                                <div className="form-group">
                                    <label>Username:</label>
                                    <input
                                        type="text"
                                        value={editUserData.username}
                                        onChange={(e) => setEditUserData({ ...editUserData, username: e.target.value })}
                                    />
                                </div>
                                <div className="form-group">
                                    <label>Email:</label>
                                    <input
                                        type="email"
                                        value={editUserData.email}
                                        onChange={(e) => setEditUserData({ ...editUserData, email: e.target.value })}
                                    />
                                </div>
                                <div className="button-group">
                                    <button className="btn-save" onClick={handleSaveUser}>Save</button>
                                    <button className="btn-cancel" onClick={() => setEditUserMode(false)}>Cancel</button>
                                </div>
                            </div>
                        ) : (
                            <div className="user-header">
                                <h3>User: {selectedUser.username} <span className="user-id">#{selectedUser.userId}</span></h3>
                                <p>Email: {selectedUser.email}</p>
                                <div className="user-actions">
                                    <button className="btn-edit" onClick={() => handleEditUserClick(selectedUser)}>Edit User</button>
                                    <button className="btn-delete" onClick={() => handleDeleteUser(selectedUser.userId)}>Delete User</button>
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="files-section">
                        <h4>Files</h4>
                        {loading ? <p>Loading files...</p> : userFiles.length === 0 ? <p>No files found for this user.</p> : (
                            <ul className="files-list">
                                {userFiles.map(file => (
                                    <li key={file.id} className="file-item">
                                        {editFileId === file.id ? (
                                            <div className="edit-file-inline">
                                                <input
                                                    type="text"
                                                    value={editFileTitle}
                                                    onChange={(e) => setEditFileTitle(e.target.value)}
                                                />
                                                <button className="btn-small save" onClick={() => handleSaveFile(file.id)}>💾</button>
                                                <button className="btn-small cancel" onClick={() => setEditFileId(null)}>❌</button>
                                            </div>
                                        ) : (
                                            <>
                                                <span className="file-info">
                                                    <strong>{file.title}</strong>
                                                    <span className="file-type">.{file.fileType}</span>
                                                </span>
                                                <div className="file-actions">
                                                    <button className="btn-small edit" onClick={() => startEditFile(file)}>✏️</button>
                                                    <button className="btn-small delete" onClick={() => handleDeleteFile(file.id)}>🗑️</button>
                                                </div>
                                            </>
                                        )}
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

export default AdminPanel;
