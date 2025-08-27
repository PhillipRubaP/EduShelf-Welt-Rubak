import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import './App.css';
import Files from './components/Files';
import Chat from './components/Chat';
import Quiz from './components/Quiz';
import Lernkarten from './components/Lernkarten';
import Login from './components/Login';
import Register from './components/Register';
import EditUser from './components/EditUser';
import MainDashboard from './components/MainDashboard';

function App() {
  const [loggedInUser, setLoggedInUser] = useState(null);
  const [activeTab, setActiveTab] = useState('dashboard');

  useEffect(() => {
    const user = localStorage.getItem('user');
    if (user) {
      setLoggedInUser(JSON.parse(user));
    }
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
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
                  <button className={activeTab === 'dashboard' ? 'active' : ''} onClick={() => setActiveTab('dashboard')}>Dashboard</button>
                  <button className={activeTab === 'files' ? 'active' : ''} onClick={() => setActiveTab('files')}>Dateien</button>
                  <button className={activeTab === 'chat' ? 'active' : ''} onClick={() => setActiveTab('chat')}>Chat</button>
                  <button className={activeTab === 'quiz' ? 'active' : ''} onClick={() => setActiveTab('quiz')}>Quiz</button>
                  <button className={activeTab === 'lernkarten' ? 'active' : ''} onClick={() => setActiveTab('lernkarten')}>Lernkarten</button>
                  <Link to="/edit-user" className="nav-icon-button">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
                  </Link>
                  <button onClick={handleLogout} className="logout-button">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
                  </button>
                </nav>
              </header>
              <main className="main-content">
                {activeTab === 'dashboard' && <MainDashboard />}
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
