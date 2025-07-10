import { useState } from 'react';
import './App.css';
import Files from './components/Files';
import Chat from './components/Chat';
import Quiz from './components/Quiz';
import Lernkarten from './components/Lernkarten';

function App() {
  const [activeTab, setActiveTab] = useState('files');

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
        {activeTab === 'files' && <Files />}
        {activeTab === 'chat' && <Chat />}
        {activeTab === 'quiz' && <Quiz />}
        {activeTab === 'lernkarten' && <Lernkarten />}
      </main>
    </div>
  );
}

export default App;
