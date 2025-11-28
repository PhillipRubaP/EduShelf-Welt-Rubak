import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import './App.css';
import api from './services/api';
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
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const checkUserSession = async () => {
      try {
        // api.js has been configured to include credentials
        const user = await api.get('/Users/me');
        if (user && user.userId) {
          setLoggedInUser(user);
          localStorage.setItem('user', JSON.stringify(user));
        } else {
          // No valid session, clear local storage
          localStorage.removeItem('user');
          setLoggedInUser(null);
        }
      } catch (error) {
        // Likely a 401 Unauthorized error
        console.error('Session check failed:', error);
        localStorage.removeItem('user');
        setLoggedInUser(null);
      } finally {
        setLoading(false);
      }
    };

    checkUserSession();
  }, []);

  useEffect(() => {
    const currentTheme = localStorage.getItem('theme');
    if (currentTheme === 'light') {
      document.documentElement.setAttribute('data-theme', 'light');
    }
  }, []);

  const handleLogout = async () => {
    try {
      await api.post('/Users/logout');
    } catch (error) {
      console.error('Error during logout:', error);
    } finally {
      localStorage.removeItem('user');
      setLoggedInUser(null);
    }
  };

  if (loading) {
    return <div>Loading...</div>; // Or a spinner component
  }

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
