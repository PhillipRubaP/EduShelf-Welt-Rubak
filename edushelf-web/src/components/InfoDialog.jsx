import React from 'react';
import './InfoDialog.css';

const InfoDialog = ({ message, onClose }) => {
  if (!message) {
    return null;
  }

  return (
    <div className="info-dialog-overlay">
      <div className="info-dialog">
        <p>{message}</p>
        <div className="dialog-buttons">
          <button onClick={onClose} className="primary">Close</button>
        </div>
      </div>
    </div>
  );
};

export default InfoDialog;