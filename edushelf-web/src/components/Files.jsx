import React, { useState, useEffect } from 'react';
import UploadDialog from './UploadDialog';
import API_BASE_URL from '../config';

const Files = () => {
  const [isUploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [files, setFiles] = useState([]);

  const fetchFiles = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/Documents`);
      if (response.ok) {
        const data = await response.json();
        setFiles(data);
      }
    } catch (error) {
      console.error('Error fetching files:', error);
    }
  };

  useEffect(() => {
    fetchFiles();
  }, []);

  const handleUploadSuccess = () => {
    setUploadDialogOpen(false);
    fetchFiles();
  };

  return (
    <div>
      <h2>Dateien</h2>
      <ul>
        {files.map((file) => (
          <li key={file.id}>{file.title}</li>
        ))}
      </ul>
      <button onClick={() => setUploadDialogOpen(true)}>Datei hochladen</button>
      {isUploadDialogOpen && <UploadDialog onClose={() => setUploadDialogOpen(false)} onUploadSuccess={handleUploadSuccess} />}
    </div>
  );
};

export default Files;