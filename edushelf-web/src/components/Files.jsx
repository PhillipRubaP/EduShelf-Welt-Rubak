import React, { useState, useEffect } from 'react';
import { FaEye, FaTrash, FaDownload, FaShareAlt, FaCheck, FaTimes, FaEdit } from 'react-icons/fa';
import UploadDialog from './UploadDialog';
import FileViewer from './FileViewer';
import ShareFileModal from './ShareFileModal';
import EditDocumentModal from './EditDocumentModal';
import api, { shareDocument, searchDocuments, getDocuments } from '../services/api';
import './Files.css';

const Files = () => {
  const [isUploadDialogOpen, setUploadDialogOpen] = useState(false);
  const [files, setFiles] = useState([]);
  const [selectedFile, setSelectedFile] = useState(null);
  const [openMenuId, setOpenMenuId] = useState(null);
  const [viewFile, setViewFile] = useState(null);

  // Sharing State
  const [isShareModalOpen, setShareModalOpen] = useState(false);
  const [fileToShare, setFileToShare] = useState(null);
  const [acceptedFiles, setAcceptedFiles] = useState([]);
  const [rejectedFiles, setRejectedFiles] = useState([]);

  const [searchTerm, setSearchTerm] = useState('');
  const [searchTag, setSearchTag] = useState('');

  // Edit State
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [fileToEdit, setFileToEdit] = useState(null);

  // Pagination State
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);

  // Load accepted/rejected states from local storage on mount
  useEffect(() => {
    const storedAccepted = JSON.parse(localStorage.getItem('edushelf_accepted_shares') || '[]');
    const storedRejected = JSON.parse(localStorage.getItem('edushelf_rejected_shares') || '[]');
    setAcceptedFiles(storedAccepted);
    setRejectedFiles(storedRejected);
  }, []);

  const fetchFiles = async () => {
    try {
      let data;
      // If no search term and no tag, use the standard getDocuments endpoint (more reliable for default view)
      if (!searchTerm && !searchTag) {
        data = await getDocuments(currentPage, pageSize);
      } else {
        data = await searchDocuments(searchTerm, searchTag, currentPage, pageSize);
      }

      setFiles(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (error) {
      console.error('Error fetching files:', error);
    }
  };

  useEffect(() => {
    fetchFiles();
  }, [currentPage, pageSize, searchTerm, searchTag]); // Refetch when any of these change

  const handleUploadSuccess = () => {
    setUploadDialogOpen(false);
    fetchFiles();
  };

  const handleFileClick = (file) => {
    setSelectedFile(file);
  };

  const handleView = (file) => {
    setViewFile(file);
  };

  const handleDelete = async (file) => {
    if (file.isShared) {
      // For shared files, "Delete" just hides it (rejects it)
      handleReject(file.id);
      return;
    }
    try {
      await api.delete(`/documents/${file.id}`);
      fetchFiles();
    } catch (error) {
      console.error('Error deleting file:', error);
    }
  };

  const handleDownload = async (file) => {
    try {
      const blob = await api.download(`/documents/download/${file.id}`);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${file.title}.${file.fileType}`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (error) {
      console.error('Error downloading file:', error);
    }
  };

  const handleShareClick = (file) => {
    setFileToShare(file);
    setShareModalOpen(true);
    setOpenMenuId(null); // Close menu
  };

  const onShareSubmit = async (emailOrUsername) => {
    if (!fileToShare) return;
    try {
      await shareDocument(fileToShare.id, emailOrUsername);
      setShareModalOpen(false);
      setFileToShare(null);
      alert(`File shared successfully with ${emailOrUsername}`);
    } catch (error) {
      alert('Failed to share file: ' + error.message);
    }
  };

  const handleAccept = (fileId) => {
    const newAccepted = [...acceptedFiles, fileId];
    setAcceptedFiles(newAccepted);
    localStorage.setItem('edushelf_accepted_shares', JSON.stringify(newAccepted));
    fetchFiles(); // Refresh to potentially show the file if it matches filters
  };

  const handleReject = (fileId) => {
    const newRejected = [...rejectedFiles, fileId];
    setRejectedFiles(newRejected);
    localStorage.setItem('edushelf_rejected_shares', JSON.stringify(newRejected));
    fetchFiles(); // Refresh to hide
  };

  const toggleMenu = (fileId) => {
    setOpenMenuId(openMenuId === fileId ? null : fileId);
  };

  const filteredFiles = files.filter(file => {
    // Client-side visual filtering for shared/rejected status
    // Search Term and Tag are handled by backend now

    // Hide rejected files
    if (rejectedFiles.includes(file.id)) return false;

    // Show if:
    // 1. Not shared (owned by user)
    // 2. Shared AND accepted
    if (!file.isShared) return true;
    if (file.isShared && acceptedFiles.includes(file.id)) return true;

    return false;
  });

  const pendingShares = files.filter(file =>
    file.isShared &&
    !acceptedFiles.includes(file.id) &&
    !rejectedFiles.includes(file.id)
  );

  const handlePreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage(prev => prev - 1);
    }
  };

  const handleNextPage = () => {
    if (currentPage < totalPages) {
      setCurrentPage(prev => prev + 1);
    }
  };

  return (
    <div className="files-container">
      {viewFile ? (
        <FileViewer file={viewFile} onClose={() => setViewFile(null)} />
      ) : (
        <div className="file-list">
          <div className="file-list-header">
            <h2>Files</h2>
            <div className="search-bar-container">
              <input
                type="text"
                placeholder="Search files..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="search-input"
              />
              <input
                type="text"
                placeholder="Filter by tag..."
                value={searchTag}
                onChange={(e) => setSearchTag(e.target.value)}
                className="search-input tag-input"
              />
            </div>
            <button onClick={() => setUploadDialogOpen(true)} className="add-file-button">+</button>
          </div>

          {/* Pending Shares Section */}
          {pendingShares.length > 0 && (
            <div className="pending-shares-section">
              <h3><FaShareAlt /> Pending Shares</h3>
              <div className="file-grid">
                {pendingShares.map(file => (
                  <div key={file.id} className="file-card pending-card">
                    <div className="shared-badge" title={`Shared by ${file.ownerName}`}>
                      <FaShareAlt size={12} />
                    </div>
                    <p>{file.title}</p>
                    <div className="file-meta">From: {file.ownerName}</div>
                    <div className="file-card-buttons">
                      <button className="btn-accept" onClick={() => handleAccept(file.id)} title="Accept">
                        <FaCheck />
                      </button>
                      <button className="btn-reject" onClick={() => handleReject(file.id)} title="Reject">
                        <FaTimes />
                      </button>
                      <button onClick={() => handleView(file)} title="View"><FaEye /></button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="file-grid">
            {filteredFiles.map((file) => (
              <div key={file.id} className="file-card">
                {file.isShared && (
                  <div className="shared-badge" title={`Shared by ${file.ownerName}`}>
                    <FaShareAlt size={12} />
                  </div>
                )}
                <p>{file.title}</p>
                <div className="file-meta-row">
                  <span className="file-meta">{file.fileType}</span>
                  {file.tags && file.tags.length > 0 && (
                    <div className="file-tags">
                      {file.tags.map(t => (
                        <span key={t.id} className="file-tag">{t.name}</span>
                      ))}
                    </div>
                  )}
                </div>
                {file.isShared && <small className="file-owner">Shared by {file.ownerName}</small>}

                <div className="file-card-buttons">
                  <button className="menu-button" onClick={() => toggleMenu(file.id)}>
                    <div className="menu-icon"></div>
                    <div className="menu-icon"></div>
                    <div className="menu-icon"></div>
                  </button>
                  {openMenuId === file.id && (
                    <div className="dropdown-menu">
                      <button onClick={() => handleView(file)} title="View"><FaEye /> View</button>

                      {!file.isShared && (
                        <button onClick={() => {
                          setFileToEdit(file);
                          setIsEditOpen(true);
                          setOpenMenuId(null);
                        }} title="Edit"><FaEdit /> Edit</button>
                      )}

                      {!file.isShared && (
                        <button onClick={() => handleShareClick(file)} title="Share"><FaShareAlt /> Share</button>
                      )}

                      <button onClick={() => handleDelete(file)} title={file.isShared ? "Hide" : "Delete"}>
                        <FaTrash /> {file.isShared ? "Hide" : "Delete"}
                      </button>

                      <button onClick={() => handleDownload(file)} title="Download"><FaDownload /> Download</button>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>

          {/* Pagination Controls */}
          {totalPages > 1 && (
            <div className="pagination-controls">
              <button
                onClick={handlePreviousPage}
                disabled={currentPage === 1}
                className="pagination-button"
              >
                Previous
              </button>
              <span className="pagination-info">
                Page {currentPage} of {totalPages}
              </span>
              <button
                onClick={handleNextPage}
                disabled={currentPage === totalPages}
                className="pagination-button"
              >
                Next
              </button>
            </div>
          )}

          {isUploadDialogOpen && <UploadDialog onClose={() => setUploadDialogOpen(false)} onUploadSuccess={handleUploadSuccess} />}

          <ShareFileModal
            isOpen={isShareModalOpen}
            onClose={() => setShareModalOpen(false)}
            onShare={onShareSubmit}
            fileName={fileToShare?.title}
          />

          {isEditOpen && fileToEdit && (
            <EditDocumentModal
              file={fileToEdit}
              onClose={() => setIsEditOpen(false)}
              onSave={() => {
                fetchFiles();
                // setIsEditOpen(false); // EditDocumentModal likely closes itself or calls onClose
              }}
            />
          )}
        </div>
      )}
    </div>
  );
};

export default Files;