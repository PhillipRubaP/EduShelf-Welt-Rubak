import { Outlet, NavLink, useNavigate } from 'react-router-dom';

const MainLayout = ({ handleLogout }) => {
  const navigate = useNavigate();

  return (
    <div className="app-container">
      <header className="navbar">
        <h1 className="navbar-title">EduShelf</h1>
        <nav className="navbar-nav">
          <NavLink to="/" end className={({ isActive }) => (isActive ? 'active' : '')}>Dashboard</NavLink>
          <NavLink to="/files" className={({ isActive }) => (isActive ? 'active' : '')}>Dateien</NavLink>
          <NavLink to="/chat" className={({ isActive }) => (isActive ? 'active' : '')}>Chat</NavLink>
          <NavLink to="/quiz" className={({ isActive }) => (isActive ? 'active' : '')}>Quiz</NavLink>
          <NavLink to="/lernkarten" className={({ isActive }) => (isActive ? 'active' : '')}>Lernkarten</NavLink>
          <button onClick={() => navigate('/edit-user')} className="nav-icon-button">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path><circle cx="12" cy="7" r="4"></circle></svg>
          </button>
          <button onClick={handleLogout} className="logout-button">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path><polyline points="16 17 21 12 16 7"></polyline><line x1="21" y1="12" x2="9" y2="12"></line></svg>
          </button>
        </nav>
      </header>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
};

export default MainLayout;