import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import './App.css';
import Files from './components/Files';
import Chat from './components/Chat';
import Quiz from './components/Quiz';
import Flashcards from './components/Flashcards';
import Login from './components/Login';
import Register from './components/Register';
import EditUser from './components/EditUser';
import Settings from './components/Settings';
import MainDashboard from './components/MainDashboard';
import MainLayout from './components/MainLayout';

function App() {
  const [loggedInUser, setLoggedInUser] = useState(null);
  const [activeTab, setActiveTab] = useState('dashboard');

  useEffect(() => {
    const user = localStorage.getItem('user');
    if (user) {
      setLoggedInUser(JSON.parse(user));
    }
  }, []);

  useEffect(() => {
    const currentTheme = localStorage.getItem('theme');
    if (currentTheme === 'light') {
        document.documentElement.setAttribute('data-theme', 'light');
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
        <Route
          path="/*"
          element={
            loggedInUser ? (
              <Routes>
                <Route element={<MainLayout handleLogout={handleLogout} />}>
                  <Route index element={<MainDashboard />} />
                  <Route path="files" element={<Files />} />
                  <Route path="chat" element={<Chat />} />
                  <Route path="quizzes" element={<Quiz />} />
                  <Route path="quiz/:quizTitle" element={<Quiz />} />
                  <Route path="flashcards" element={<Flashcards />} />
                  <Route path="edit-user" element={<EditUser loggedInUser={loggedInUser} setLoggedInUser={setLoggedInUser} />} />
                  <Route path="settings" element={<Settings />} />
                  <Route path="*" element={<Navigate to="/" />} />
                </Route>
              </Routes>
            ) : (
              <Navigate to="/login" />
            )
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
