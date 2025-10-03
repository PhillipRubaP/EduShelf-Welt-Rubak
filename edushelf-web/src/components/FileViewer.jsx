import React, { useState, useEffect } from 'react';
import api from '../services/api';

const FileViewer = ({ document }) => {
  const [content, setContent] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    if (document) {
      const fetchDocumentContent = async () => {
        try {
          const response = await api.get(`/documents/download/${document.id}`, {
            responseType: 'blob',
          });
          const file = new Blob([response], { type: response.headers['content-type'] });
          const text = await file.text();
          setContent(text);
          setError('');
        } catch (err) {
          setError('Error fetching document content.');
          console.error(err);
        }
      };
      fetchDocumentContent();
    }
  }, [document]);

  const renderContent = () => {
    if (error) {
      return <p>{error}</p>;
    }
    if (!content) {
      return <p>Loading content...</p>;
    }

    const fileType = document.title.split('.').pop().toLowerCase();

    switch (fileType) {
      case 'pdf':
        return <p>PDF preview not yet implemented.</p>;
      case 'md':
        return <pre>{content}</pre>;
      case 'txt':
        return <pre>{content}</pre>;
      default:
        return <p>Unsupported file type.</p>;
    }
  };

  return (
    <div className="file-viewer p-4">
      <h3 className="text-xl font-bold mb-2">{document.title}</h3>
      <div className="content bg-gray-100 p-4 rounded">
        {renderContent()}
      </div>
    </div>
  );
};

export default FileViewer;