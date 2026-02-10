import React, { useState, useEffect } from 'react';
import { getDocuments, getQuizzes, getFlashcards, createFlashcard } from '../services/api';
import FileViewer from './FileViewer';
import UploadDialog from './UploadDialog';
import QuizModal from './QuizModal';
import FlashcardsModal from './FlashcardsModal';
import './MainDashboard.css';

function MainDashboard() {
  const [recentFiles, setRecentFiles] = useState([]);
  const [stats, setStats] = useState({
    documents: 0,
    quizzes: 0,
    flashcards: 0
  });
  const [selectedFile, setSelectedFile] = useState(null);

  // Modal states
  const [isUploadOpen, setIsUploadOpen] = useState(false);
  const [isQuizOpen, setIsQuizOpen] = useState(false);
  const [isFlashcardOpen, setIsFlashcardOpen] = useState(false);

  const fetchData = async () => {
    try {
      const [docsData, quizzes, flashcards] = await Promise.all([
        getDocuments(),
        getQuizzes(),
        getFlashcards()
      ]);

      const docs = docsData.items || docsData;

      if (Array.isArray(docs)) {
        // Take only the first 3 documents
        setRecentFiles(docs.slice(0, 3));
      }

      setStats({
        documents: docsData.totalCount || (Array.isArray(docs) ? docs.length : 0),
        quizzes: quizzes.length,
        flashcards: flashcards.totalCount || (Array.isArray(flashcards) ? flashcards.length : 0)
      });
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Quick Action Handlers
  const handleUploadSuccess = () => {
    fetchData(); // Refresh data to show new file in stats/recent
    setIsUploadOpen(false);
  };

  const handleQuizSaved = (newQuiz) => {
    fetchData(); // Refresh stats
    setIsQuizOpen(false);
  };

  const handleAddFlashcard = async (cardData) => {
    try {
      await createFlashcard(cardData);
      fetchData(); // Refresh stats
      setIsFlashcardOpen(false);
    } catch (error) {
      console.error("Error creating flashcard:", error);
    }
  };

  const handleFileClick = (file) => {
    setSelectedFile(file);
  };

  const handleCloseViewer = () => {
    setSelectedFile(null);
  };

  return (
    <div className="main-dashboard">
      <header className="dashboard-header">
        <h1>Welcome Back, Scholar!</h1>
        <p className="subtitle">Here's what's happening in your digital library.</p>
      </header>

      <div className="dashboard-grid">
        {/* Recently Added Files */}
        <section className="dashboard-card recent-files">
          <div className="card-header">
            <h2>Recently Added</h2>
            <span className="badge">Last 3</span>
          </div>
          {recentFiles.length > 0 ? (
            <ul className="file-list">
              {recentFiles.map((file) => (
                <li key={file.id} className="file-item" onClick={() => handleFileClick(file)}>
                  <div className="file-icon">📄</div>
                  <div className="file-info">
                    <span className="file-title">{file.title}</span>
                    <span className="file-meta">{file.fileType}</span>
                  </div>
                  <button className="view-btn">View</button>
                </li>
              ))}
            </ul>
          ) : (
            <div className="empty-state">
              <p>No recently viewed files.</p>
            </div>
          )}
        </section>

        {/* Quick Stats */}
        <section className="dashboard-card stats-card">
          <h2>Study Stats</h2>
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-value">{stats.documents}</span>
              <span className="stat-label">Documents</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{stats.quizzes}</span>
              <span className="stat-label">Quizzes</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{stats.flashcards}</span>
              <span className="stat-label">Flashcards</span>
            </div>
          </div>
        </section>

        {/* Quick Actions */}
        <section className="dashboard-card quick-actions">
          <h2>Quick Actions</h2>
          <div className="actions-grid">
            <button className="action-btn upload" onClick={() => setIsUploadOpen(true)}>
              <span className="action-icon">⬆️</span>
              Upload File
            </button>
            <button className="action-btn quiz" onClick={() => setIsQuizOpen(true)}>
              <span className="action-icon">📝</span>
              Create Quiz
            </button>
            <button className="action-btn flashcard" onClick={() => setIsFlashcardOpen(true)}>
              <span className="action-icon">🗂️</span>
              New Flashcard
            </button>
          </div>
        </section>
      </div>

      {selectedFile && <FileViewer file={selectedFile} onClose={handleCloseViewer} />}

      {/* Modals */}
      {isUploadOpen && (
        <UploadDialog
          onClose={() => setIsUploadOpen(false)}
          onUploadSuccess={handleUploadSuccess}
        />
      )}

      {isQuizOpen && (
        <QuizModal
          onClose={() => setIsQuizOpen(false)}
          onQuizSaved={handleQuizSaved}
        />
      )}

      {isFlashcardOpen && (
        <FlashcardsModal
          closeModal={() => setIsFlashcardOpen(false)}
          addCard={handleAddFlashcard}
          card={null}
          updateCard={() => { }} // Not needed for create
        />
      )}
    </div>
  );
}

export default MainDashboard;