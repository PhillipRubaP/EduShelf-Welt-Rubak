import React, { useState } from 'react';
import './UploadDialog.css';
import api from '../services/api';

const UploadDialog = ({ onClose, onUploadSuccess }) => {
  const [selectedFile, setSelectedFile] = useState(null);
  const [tags, setTags] = useState([]);
  const [currentTag, setCurrentTag] = useState('');

  const handleFileChange = (event) => {
    setSelectedFile(event.target.files[0]);
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      alert('Please select a file first!');
      return;
    }

    const formData = new FormData();
    formData.append('file', selectedFile);
    const user = JSON.parse(localStorage.getItem('user'));
    formData.append('userId', user.userId);

    tags.forEach(tag => formData.append('tags', tag));

    try {
      const response = await api.postForm('/Documents', formData);
      if (response) {
        alert('File uploaded successfully!');
        if (onUploadSuccess) {
          onUploadSuccess();
        }
        onClose();
      } else {
        alert('File upload failed.');
      }
    } catch (error) {
      console.error('Error uploading file:', error);
      alert('An error occurred while uploading the file.');
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
          <h2 className="modal-title">Upload File</h2>
        </div>

        <div className="modal-body">
          <div className="form-group">
            <label className="form-label">Select File</label>
            <input
              type="file"
              onChange={handleFileChange}
              accept=".pdf,.doc,.docx,.txt"
              className="form-input"
              style={{ paddingTop: '0.5rem' }} // Minor adjustment for file input
            />
          </div>

          <div className="form-group">
            <label className="form-label">Tags</label>
            <div className="tags-input-container">
              <div className="tags-list">
                {tags.map((tag, index) => (
                  <span key={index} className="tag-pill">
                    {tag}
                    <button type="button" onClick={() => setTags(tags.filter(t => t !== tag))} className="tag-remove">×</button>
                  </span>
                ))}
              </div>
              <div className="input-with-button">
                <input
                  type="text"
                  value={currentTag}
                  onChange={(e) => setCurrentTag(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && currentTag.trim()) {
                      e.preventDefault();
                      if (!tags.includes(currentTag.trim())) {
                        setTags([...tags, currentTag.trim()]);
                        setCurrentTag('');
                      }
                    }
                  }}
                  placeholder="Add tags..."
                  className="form-input"
                />
                <button
                  type="button"
                  onClick={() => {
                    if (currentTag.trim() && !tags.includes(currentTag.trim())) {
                      setTags([...tags, currentTag.trim()]);
                      setCurrentTag('');
                    }
                  }}
                  className="btn btn-secondary"
                  style={{ padding: '0.5rem' }}
                >
                  Add
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="modal-footer">
          <button className="btn btn-secondary" onClick={onClose}>
            Cancel
          </button>
          <button className="btn btn-primary" onClick={handleUpload}>
            Upload
          </button>
        </div>
      </div>
    </div>
  );
};

export default UploadDialog;