import { useState, useEffect } from 'react';
import PublicPage from './components/PublicPage';
import AdminPage from './components/AdminPage';
import './App.css';

function getIsAdmin(): boolean {
  const path = window.location.pathname;
  return /\/admin\/?$/.test(path);
}

function App() {
  const [isAdmin, setIsAdmin] = useState(getIsAdmin);

  useEffect(() => {
    function onPopState() {
      setIsAdmin(getIsAdmin());
    }
    window.addEventListener('popstate', onPopState);
    return () => window.removeEventListener('popstate', onPopState);
  }, []);

  return (
    <div className="app">
      <header className="app-header">
        <h1>🌑 Moony</h1>
        <span className="subtitle">EVE Online Moon Scan Formatter</span>
      </header>
      {isAdmin ? <AdminPage /> : <PublicPage />}
    </div>
  );
}

export default App;
