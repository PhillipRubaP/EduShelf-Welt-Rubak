import React, { useState, useEffect } from 'react';
import { getDocuments } from '../services/api';
import FileViewer from './FileViewer';
import './MainDashboard.css';

function MainDashboard() {
  const [recentFiles, setRecentFiles] = useState([]);
  const [selectedFile, setSelectedFile] = useState(null);

  useEffect(() => {
    const fetchDocuments = async () => {
      try {
        const documents = await getDocuments();
        setRecentFiles(documents);
      } catch (error) {
        console.error('Error fetching documents:', error);
      }
    };

    fetchDocuments();
  }, []);

  const handleFileClick = (file) => {
    setSelectedFile(file);
  };

  const handleCloseViewer = () => {
    setSelectedFile(null);
  };

  return (
    <div className="main-dashboard">
      <h1>Main Dashboard</h1>
      <div className="dashboard-section">
        <h2>Recently Viewed Files</h2>
        <ul className="file-list">
          {recentFiles.map((file) => (
            <li key={file.id} className="file-item" onClick={() => handleFileClick(file)}>
              <span className="file-title">{file.title}</span>
              <span className="file-type">{file.fileType}</span>
            </li>
          ))}
        </ul>
      </div>
      {selectedFile && <FileViewer file={selectedFile} onClose={handleCloseViewer} />}
    </div>
  );
}

export default MainDashboard;