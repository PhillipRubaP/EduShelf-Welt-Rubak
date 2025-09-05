import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Files from './components/Files';
import Chat from './components/Chat';
import Quiz from './components/Quiz';
import Lernkarten from './components/Lernkarten';
import Login from './components/Login';
import Register from './components/Register';
import EditUser from './components/EditUser';
import MainDashboard from './components/MainDashboard';
import MainLayout from './components/MainLayout';

function App() {
  const [loggedInUser, setLoggedInUser] = useState(null);

  const handleLogout = () => {
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
                  <Route path="quiz" element={<Quiz />} />
                  <Route path="lernkarten" element={<Lernkarten />} />
                  <Route path="edit-user" element={<EditUser loggedInUser={loggedInUser} setLoggedInUser={setLoggedInUser} />} />
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
