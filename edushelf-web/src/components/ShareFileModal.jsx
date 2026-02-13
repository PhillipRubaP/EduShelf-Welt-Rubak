import React, { useState } from 'react';
import './ShareFileModal.css';

const ShareFileModal = ({ isOpen, onClose, onShare, fileName }) => {
    const [emailOrUsername, setEmailOrUsername] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    if (!isOpen) return null;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!emailOrUsername.trim()) return;

        setIsLoading(true);
        try {
            await onShare(emailOrUsername);
            setEmailOrUsername(''); // Reset input on success
        } catch (error) {
            // Error handling is done in parent
            console.error(error);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="share-file-modal-overlay">
            <div className="share-file-modal">
                <h2>
                    Share "{fileName}"
                </h2>

                <form onSubmit={handleSubmit} className="share-file-form">
                    <div className="form-group">
                        <label htmlFor="userIdent">Share with (Email or Username)</label>
                        <input
                            id="userIdent"
                            type="text"
                            value={emailOrUsername}
                            onChange={(e) => setEmailOrUsername(e.target.value)}
                            placeholder="Enter email or username..."
                            autoFocus
                            required
                        />
                    </div>

                    <div className="modal-actions">
                        <button
                            type="button"
                            className="btn-cancel"
                            onClick={onClose}
                            disabled={isLoading}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            className="btn-share"
                            disabled={!emailOrUsername.trim() || isLoading}
                        >
                            {isLoading ? 'Sharing...' : 'Share File'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default ShareFileModal;
