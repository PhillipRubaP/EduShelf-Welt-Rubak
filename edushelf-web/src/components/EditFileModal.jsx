import React, { useState, useEffect } from 'react';
import './UploadDialog.css'; // Reuse existing modal styles

const EditFileModal = ({ file, api, onClose, onUpdateSuccess }) => {
    const [title, setTitle] = useState('');
    const [tags, setTags] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (file) {
            setTitle(file.title);
            // Check if tags are objects (from backend) or strings
            const tagStrings = file.tags
                ? file.tags.map(t => typeof t === 'object' ? t.name : t)
                : [];
            setTags(tagStrings.join(', '));
        }
    }, [file]);

    const handleSave = async (e) => {
        e.preventDefault();
        setLoading(true);

        const tagsArray = tags.split(',').map(tag => tag.trim()).filter(tag => tag);

        try {
            // API likely expects PascalCase for the properties as seen in Flashcards
            // We ensure we send ONLY what is needed to avoid binding errors with duplicate/extra keys
            const updatedDoc = {
                Title: title,
                Tags: tagsArray
            };
            console.log('Sending clean updatedDoc:', updatedDoc);

            await api.put(`/Documents/${file.id}`, updatedDoc);
            onUpdateSuccess();
            onClose();
        } catch (error) {
            console.error('Error updating file:', error);
            alert('Failed to update file.');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="modal-overlay">
            <div className="modal-container">
                <button className="modal-close-button" onClick={onClose}>
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>

                <div className="modal-header">
                    <h2 className="modal-title">Edit File</h2>
                </div>

                <div className="modal-body">
                    <form onSubmit={handleSave} id="edit-file-form">
                        <div className="form-group">
                            <label className="form-label" htmlFor="title">Title</label>
                            <input
                                type="text"
                                id="title"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                className="form-input"
                                required
                            />
                        </div>
                        <div className="form-group">
                            <label className="form-label" htmlFor="tags">Tags</label>
                            <input
                                type="text"
                                id="tags"
                                value={tags}
                                onChange={(e) => setTags(e.target.value)}
                                className="form-input"
                                placeholder="e.g. History, Notes (comma separated)"
                            />
                        </div>
                    </form>
                </div>

                <div className="modal-footer">
                    <button type="button" className="btn btn-secondary" onClick={onClose} disabled={loading}>
                        Cancel
                    </button>
                    <button type="submit" form="edit-file-form" className="btn btn-primary" disabled={loading}>
                        {loading ? 'Saving...' : 'Save Changes'}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default EditFileModal;
