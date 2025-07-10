import React from 'react';
import './UploadDialog.css';

const UploadDialog = ({ onClose }) => {
  return (
    <div className="upload-dialog-overlay">
      <div className="upload-dialog">
        <h2>Datei hochladen</h2>
        <input type="file" />
        <div className="dialog-buttons">
          <button onClick={onClose}>Schließen</button>
          <button className="primary">Hochladen</button>
        </div>
      </div>
    </div>
  );
};

export default UploadDialog;