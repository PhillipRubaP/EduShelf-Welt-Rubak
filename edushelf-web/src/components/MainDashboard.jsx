import { useState, useEffect } from 'react';
import { getDocuments, getQuizzes, getFlashcards, createFlashcard } from '../services/api';
import FileViewer from './FileViewer';
import UploadDialog from './UploadDialog';
import QuizModal from './QuizModal';
import FlashcardsModal from './FlashcardsModal';
import EditDocumentModal from './EditDocumentModal';
import './MainDashboard.css';

function MainDashboard() {
  const [recentFiles, setRecentFiles] = useState([]);
  const [stats, setStats] = useState({ documents: 0, quizzes: 0, flashcards: 0 });
  const [selectedFile, setSelectedFile] = useState(null);
  const [isUploadOpen, setIsUploadOpen] = useState(false);
  const [isQuizOpen, setIsQuizOpen] = useState(false);
  const [isFlashcardOpen, setIsFlashcardOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [fileToEdit, setFileToEdit] = useState(null);

  const fetchData = async () => {
    try {
      const [docsData, quizzes, flashcards] = await Promise.all([
        getDocuments(),
        getQuizzes(),
        getFlashcards(),
      ]);

      let docs = docsData.items || docsData;

      if (Array.isArray(docs)) {
        const acceptedShares = JSON.parse(localStorage.getItem('edushelf_accepted_shares') || '[]');
        const rejectedShares = JSON.parse(localStorage.getItem('edushelf_rejected_shares') || '[]');

        docs = docs.filter(doc => {
          if (!doc.isShared) return true;
          if (rejectedShares.includes(doc.id)) return false;
          return acceptedShares.includes(doc.id);
        });

        setRecentFiles(docs.slice(0, 3));
      }

      setStats({
        documents: docsData.totalCount || (Array.isArray(docs) ? docs.length : 0),
        quizzes: quizzes.totalCount || (Array.isArray(quizzes) ? quizzes.length : 0),
        flashcards: flashcards.totalCount || (Array.isArray(flashcards) ? flashcards.length : 0),
      });
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleUploadSuccess = () => {
    fetchData();
    setIsUploadOpen(false);
  };

  const handleQuizSaved = () => {
    fetchData();
    setIsQuizOpen(false);
  };

  const handleAddFlashcard = async (cardData) => {
    try {
      await createFlashcard(cardData);
      fetchData();
      setIsFlashcardOpen(false);
    } catch (error) {
      console.error('Error creating flashcard:', error);
    }
  };

  const handleEditFile = (e, file) => {
    e.stopPropagation();
    setFileToEdit(file);
    setIsEditOpen(true);
  };

  return (
    <div className="main-dashboard">
      <header className="dashboard-header">
        <h1>Welcome Back, Scholar!</h1>
        <p className="subtitle">Here's what's happening in your digital library.</p>
      </header>

      <div className="dashboard-grid">
        <section className="dashboard-card recent-files">
          <div className="card-header">
            <h2>Recently Added</h2>
            <span className="badge">Last 3</span>
          </div>
          {recentFiles.length > 0 ? (
            <ul className="file-list">
              {recentFiles.map((file) => (
                <li key={file.id} className="file-item" onClick={() => setSelectedFile(file)}>
                  <div className="file-icon">📄</div>
                  <div className="file-info">
                    <span className="file-title">{file.title}</span>
                    <div className="file-meta-row">
                      <span className="file-meta">{file.fileType}</span>
                      {file.tags?.length > 0 && (
                        <div className="file-tags">
                          {file.tags.map(t => (
                            <span key={t.id} className="file-tag">{t.name}</span>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          ) : (
            <div className="empty-state">
              <p>No recently viewed files.</p>
            </div>
          )}
        </section>

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

      {selectedFile && <FileViewer file={selectedFile} onClose={() => setSelectedFile(null)} />}

      {isUploadOpen && (
        <UploadDialog onClose={() => setIsUploadOpen(false)} onUploadSuccess={handleUploadSuccess} />
      )}

      {isQuizOpen && (
        <QuizModal onClose={() => setIsQuizOpen(false)} onQuizSaved={handleQuizSaved} />
      )}

      {isFlashcardOpen && (
        <FlashcardsModal
          closeModal={() => setIsFlashcardOpen(false)}
          addCard={handleAddFlashcard}
          card={null}
          updateCard={() => { }}
        />
      )}

      {isEditOpen && fileToEdit && (
        <EditDocumentModal
          file={fileToEdit}
          onClose={() => setIsEditOpen(false)}
          onSave={fetchData}
        />
      )}
    </div>
  );
}

export default MainDashboard;