import React, { useState } from 'react';
import './UploadDialog.css';
import api from '../services/api';

const UploadDialog = ({ onClose, onUploadSuccess }) => {
  const [selectedFile, setSelectedFile] = useState(null);

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
    <div className="upload-dialog-overlay">
      <div className="upload-dialog">
        <h2>Datei hochladen</h2>
        <input type="file" onChange={handleFileChange} />
        <div className="dialog-buttons">
          <button onClick={onClose}>Schließen</button>
          <button onClick={handleUpload} className="primary">Hochladen</button>
        </div>
      </div>
    </div>
  );
};

export default UploadDialog;