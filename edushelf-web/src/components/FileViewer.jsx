import React, { useState, useEffect } from 'react';
import api from '../services/api';
import DocxViewer from './DocxViewer';
import './FileViewer.css';

const FileViewer = ({ file, onClose }) => {
  const [content, setContent] = useState(<p>Loading...</p>);

  useEffect(() => {
    setContent(<p>Loading...</p>);

    const fetchFile = async () => {
      if (file) {
        try {
          const blob = await api.get(`/documents/download/${file.id}`, { responseType: 'blob' });

          if (file.fileType === 'docx' || file.fileType === 'doc') {
            setContent(<DocxViewer blob={blob} />);
          } else if (file.fileType === 'txt') {
            const text = await blob.text();
            setContent(<pre>{text}</pre>);
          } else { // pdf and other types
            const url = window.URL.createObjectURL(blob);
            setContent(<iframe src={url} title="File Viewer" width="100%" height="100%" style={{ border: 'none' }}></iframe>);
          }
        } catch (error) {
          console.error('Error fetching or rendering file:', error);
          setContent(<p>Error loading file.</p>);
        }
      }
    };

    fetchFile();
  }, [file]);

  if (!file) {
    return null;
  }

  return (
    <div className="file-viewer-modal">
      <div className="file-viewer-content">
        <div className="modal-close-button" onClick={onClose}>
          <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>
        {content}
      </div>
    </div>
  );
};

export default FileViewer;