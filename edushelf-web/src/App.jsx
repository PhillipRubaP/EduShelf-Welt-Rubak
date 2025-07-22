import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import './App.css';
import Files from './components/Files';
import Chat from './components/Chat';
import Quiz from './components/Quiz';
import Lernkarten from './components/Lernkarten';
import Login from './components/Login';
import Register from './components/Register';
import EditUser from './components/EditUser';

function App() {
  const [loggedInUser, setLoggedInUser] = useState(null);
  const [activeTab, setActiveTab] = useState('files');

  const handleLogout = () => {
    setLoggedInUser(null);
  };

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login setLoggedInUser={setLoggedInUser} />} />
        <Route path="/register" element={<Register />} />
        <Route path="/edit-user" element={loggedInUser ? <EditUser loggedInUser={loggedInUser} setLoggedInUser={setLoggedInUser} /> : <Navigate to="/login" />} />
        <Route path="/" element={
          loggedInUser ? (
            <div className="app-container">
              <header className="navbar">
                <h1 className="navbar-title">EduShelf</h1>
                <nav className="navbar-nav">
                  <button className={activeTab === 'files' ? 'active' : ''} onClick={() => setActiveTab('files')}>Dateien</button>
                  <button className={activeTab === 'chat' ? 'active' : ''} onClick={() => setActiveTab('chat')}>Chat</button>
                  <button className={activeTab === 'quiz' ? 'active' : ''} onClick={() => setActiveTab('quiz')}>Quiz</button>
                  <button className={activeTab === 'lernkarten' ? 'active' : ''} onClick={() => setActiveTab('lernkarten')}>Lernkarten</button>
                  <Link to="/edit-user" className="text-white hover:text-gray-300 px-3 py-2 rounded-md text-sm font-medium">Profile</Link>
                  <button onClick={handleLogout} className="text-white hover:text-gray-300 px-3 py-2 rounded-md text-sm font-medium">Logout</button>
                </nav>
              </header>
              <main className="main-content">
                {activeTab === 'files' && <Files />}
                {activeTab === 'chat' && <Chat />}
                {activeTab === 'quiz' && <Quiz />}
                {activeTab === 'lernkarten' && <Lernkarten />}
              </main>
            </div>
          ) : (
            <Navigate to="/login" />
          )
        } />
      </Routes>
    </Router>
  );
}

export default App;
