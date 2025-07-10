import { useState } from 'react';
import './App.css';

function App() {
  const [activeTab, setActiveTab] = useState('files');

  return (
    <div>
      <h1>Welcome to EduShelf</h1>
      <nav>
        <button className={activeTab === 'files' ? 'active' : ''} onClick={() => setActiveTab('files')}>Dateien</button>
        <button className={activeTab === 'chat' ? 'active' : ''} onClick={() => setActiveTab('chat')}>Chat</button>
        <button className={activeTab === 'quiz' ? 'active' : ''} onClick={() => setActiveTab('quiz')}>Quiz</button>
        <button className={activeTab === 'lernkarten' ? 'active' : ''} onClick={() => setActiveTab('lernkarten')}>Lernkarten</button>
      </nav>
      <hr />
      <main>
        {activeTab === 'files' && <h2>Dateien</h2>}
        {activeTab === 'chat' && <h2>Chat</h2>}
        {activeTab === 'quiz' && <h2>Quiz</h2>}
        {activeTab === 'lernkarten' && <h2>Lernkarten</h2>}
      </main>
    </div>
  );
}

export default App;
