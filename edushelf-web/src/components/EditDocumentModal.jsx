import React, { useState, useEffect } from 'react';
import api, { updateDocumentTags } from '../services/api';
import './EditDocumentModal.css';

const EditDocumentModal = ({ file, onClose, onSave }) => {
    const [title, setTitle] = useState(file.title);
    const [tags, setTags] = useState(file.tags ? file.tags.map(t => t.name) : []);
    const [currentTag, setCurrentTag] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (file) {
            setTitle(file.title);
            setTags(file.tags ? file.tags.map(t => t.name) : []);
        }
    }, [file]);

    const handleAddTag = (e) => {
        if ((e.key === 'Enter' || e.type === 'click') && currentTag.trim()) {
            e.preventDefault();
            const newTag = currentTag.trim();
            if (!tags.includes(newTag)) {
                setTags([...tags, newTag]);
            }
            setCurrentTag('');
        }
    };

    const handleRemoveTag = (tagToRemove) => {
        setTags(tags.filter(tag => tag !== tagToRemove));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            // 1. Update Title
            if (title !== file.title) {
                await api.put(`/Documents/${file.id}`, { id: file.id, title: title, fileType: file.fileType });
            }

            // 2. Update Tags
            // We send the list of tag NAMES. The backend handles finding/creating them.
            await updateDocumentTags(file.id, tags);

            if (onSave) {
                onSave();
            }
            onClose();
        } catch (error) {
            console.error("Failed to update document:", error);
            alert("Failed to save changes.");
        } finally {
            setIsLoading(false);
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
                    <h2>Edit Document</h2>
                </div>
                <div className="modal-body">
                    <form onSubmit={handleSubmit}>
                        <div className="form-group">
                            <label>Title</label>
                            <input
                                type="text"
                                value={title}
                                className="form-input disabled-input"
                                disabled
                                title="File renaming is disabled"
                            />
                        </div>
                        <div className="form-group">
                            <label>Tags</label>
                            <div className="tags-input-container">
                                <div className="tags-list">
                                    {tags.map((tag, index) => (
                                        <span key={index} className="tag-pill">
                                            <span
                                                className="tag-text"
                                                onClick={() => {
                                                    setCurrentTag(tag);
                                                    handleRemoveTag(tag);
                                                    // Focus logic could be added here if we had a ref
                                                }}
                                                title="Click to edit"
                                            >
                                                {tag}
                                            </span>
                                            <button type="button" onClick={() => handleRemoveTag(tag)} className="tag-remove">×</button>
                                        </span>
                                    ))}
                                </div>
                                <div className="input-with-button">
                                    <input
                                        type="text"
                                        value={currentTag}
                                        onChange={(e) => setCurrentTag(e.target.value)}
                                        onKeyDown={handleAddTag}
                                        placeholder="Add a tag..."
                                        className="form-input"
                                    />
                                    <button type="button" onClick={handleAddTag} className="btn btn-small">Add</button>
                                </div>
                            </div>
                        </div>
                        <div className="modal-footer">
                            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={isLoading}>Cancel</button>
                            <button type="submit" className="btn btn-primary" disabled={isLoading}>
                                {isLoading ? 'Saving...' : 'Save Changes'}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default EditDocumentModal;
