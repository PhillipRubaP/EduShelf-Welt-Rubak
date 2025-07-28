import React, { useState } from 'react';
import './UploadDialog.css';
import API_BASE_URL from '../config';

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
    formData.append('userId', 1); // Replace with actual user ID

    try {
      const response = await fetch(`${API_BASE_URL}/Documents`, {
        method: 'POST',
        body: formData,
      });

      if (response.ok) {
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