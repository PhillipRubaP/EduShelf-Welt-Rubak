import React, { useState, useEffect } from 'react';
import UploadDialog from './UploadDialog';
import api from '../services/api';

const Files = () => {
  const [isUploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [files, setFiles] = useState([]);

  const fetchFiles = async () => {
    try {
      const data = await api.get('/Documents');
      setFiles(data);
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