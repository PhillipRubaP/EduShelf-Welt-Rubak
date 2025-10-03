import React, { useState, useEffect } from 'react';
import { FaEye, FaTrash, FaDownload } from 'react-icons/fa';
import UploadDialog from './UploadDialog';
import FileViewer from './FileViewer';
import api from '../services/api';
import './Files.css';

const Files = () => {
  const [isUploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [files, setFiles] = useState([]);
  const [selectedFile, setSelectedFile] = useState(null);
  const [openMenuId, setOpenMenuId] = useState(null);

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

  const handleFileClick = (file) => {
    setSelectedFile(file);
  };

  const handleView = async (file) => {
    try {
      const response = await api.get(`/documents/download/${file.id}`, {
        responseType: 'blob',
      });
      const fileURL = window.URL.createObjectURL(new Blob([response], { type: response.headers['content-type'] }));
      window.open(fileURL, '_blank');
    } catch (error) {
      console.error('Error viewing file:', error);
    }
  };

  const handleDelete = async (fileId) => {
    try {
      await api.delete(`/documents/${fileId}`);
      fetchFiles();
    } catch (error) {
      console.error('Error deleting file:', error);
    }
  };

  const handleDownload = async (file) => {
    try {
      const response = await api.get(`/documents/download/${file.id}`, {
        responseType: 'blob',
      });
      const url = window.URL.createObjectURL(new Blob([response]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', file.title);
      document.body.appendChild(link);
      link.click();
    } catch (error) {
      console.error('Error downloading file:', error);
    }
  };

  const toggleMenu = (fileId) => {
    setOpenMenuId(openMenuId === fileId ? null : fileId);
  };

  return (
    <div className="files-container">
      <div className="file-list">
        <div className="file-list-header">
          <h2>Dateien</h2>
          <button onClick={() => setUploadDialogOpen(true)} className="add-file-button">+</button>
        </div>
        <div className="file-grid">
          {files.map((file) => (
            <div key={file.id} className="file-card">
              <p>{file.title}</p>
              <div className="file-card-buttons">
                <button className="menu-button" onClick={() => toggleMenu(file.id)}>
                  <div className="menu-icon"></div>
                  <div className="menu-icon"></div>
                  <div className="menu-icon"></div>
                </button>
                {openMenuId === file.id && (
                  <div className="dropdown-menu">
                    <button onClick={() => handleView(file)} title="View"><FaEye /></button>
                    <button onClick={() => handleDelete(file.id)} title="Delete"><FaTrash /></button>
                    <button onClick={() => handleDownload(file)} title="Download"><FaDownload /></button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
        {isUploadDialogOpen && <UploadDialog onClose={() => setUploadDialogOpen(false)} onUploadSuccess={handleUploadSuccess} />}
      </div>
    </div>
  );
};

export default Files;