import { useState } from 'react';
import './App.css';
import UploadDialog from './components/UploadDialog';

function App() {
  const [activeTab, setActiveTab] = useState('files');
  const [isUploadDialogOpen, setUploadDialogOpen] = useState(false);

  return (
    <div className="app-container">
      <header className="navbar">
        <h1 className="navbar-title">EduShelf</h1>
        <nav className="navbar-nav">
          <button className={activeTab === 'files' ? 'active' : ''} onClick={() => setActiveTab('files')}>Dateien</button>
          <button className={activeTab === 'chat' ? 'active' : ''} onClick={() => setActiveTab('chat')}>Chat</button>
          <button className={activeTab === 'quiz' ? 'active' : ''} onClick={() => setActiveTab('quiz')}>Quiz</button>
          <button className={activeTab === 'lernkarten' ? 'active' : ''} onClick={() => setActiveTab('lernkarten')}>Lernkarten</button>
        </nav>
      </header>
      <main className="main-content">
        {activeTab === 'files' && (
          <div>
            <h2>Dateien</h2>
            <button onClick={() => setUploadDialogOpen(true)}>Datei hochladen</button>
          </div>
        )}
        {activeTab === 'chat' && <h2>Chat</h2>}
        {activeTab === 'quiz' && <h2>Quiz</h2>}
        {activeTab === 'lernkarten' && <h2>Lernkarten</h2>}
      </main>
      {isUploadDialogOpen && <UploadDialog onClose={() => setUploadDialogOpen(false)} />}
    </div>
  );
}

export default App;
