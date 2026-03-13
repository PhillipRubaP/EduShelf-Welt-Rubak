import { useState, useEffect } from 'react';
import api from '../services/api';
import DocxViewer from './DocxViewer';
import './FileViewer.css';

const FileViewer = ({ file, onClose }) => {
  const [content, setContent] = useState(<p>Loading...</p>);

  useEffect(() => {
    if (!file) return;
    setContent(<p>Loading...</p>);

    const fetchFile = async () => {
      try {
        const blob = await api.get(`/documents/download/${file.id}`, { responseType: 'blob' });

        if (file.fileType === 'docx' || file.fileType === 'doc') {
          setContent(<DocxViewer blob={blob} />);
        } else if (file.fileType === 'txt') {
          const text = await blob.text();
          setContent(<pre>{text}</pre>);
        } else {
          const url = window.URL.createObjectURL(blob);
          setContent(<iframe src={url} title="File Viewer" width="100%" height="100%" style={{ border: 'none' }} />);
        }
      } catch (error) {
        console.error('Error fetching or rendering file:', error);
        setContent(<p>Error loading file.</p>);
      }
    };

    fetchFile();
  }, [file]);

  if (!file) return null;

  return (
    <div className="file-viewer-modal">
      <div className="file-viewer-content">
        <button className="modal-close-button" onClick={onClose} aria-label="Close">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        {content}
      </div>
    </div>
  );
};

export default FileViewer;