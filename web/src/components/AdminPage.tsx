import { useState, useEffect } from 'react';
import type { ScanSummary, ScanDetail } from '../types';
import { checkPassword, listScans, getScan } from '../api';
import ScanResults from './ScanResults';
import ScanHistory from './ScanHistory';
import './AdminPage.css';

const SESSION_KEY = 'moony_admin_password';

export default function AdminPage() {
  const [password, setPassword] = useState(() => sessionStorage.getItem(SESSION_KEY) || '');
  const [authenticated, setAuthenticated] = useState(() => !!sessionStorage.getItem(SESSION_KEY));
  const [authError, setAuthError] = useState('');
  const [authLoading, setAuthLoading] = useState(false);
  const [passwordInput, setPasswordInput] = useState('');

  const [scans, setScans] = useState<ScanSummary[]>([]);
  const [selectedScan, setSelectedScan] = useState<ScanDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (authenticated && password) {
      loadScans();
    }
  }, [authenticated, password]);

  async function handleLogin() {
    setAuthLoading(true);
    setAuthError('');
    try {
      const ok = await checkPassword(passwordInput);
      if (ok) {
        sessionStorage.setItem(SESSION_KEY, passwordInput);
        setPassword(passwordInput);
        setAuthenticated(true);
      } else {
        setAuthError('Invalid password');
      }
    } catch {
      setAuthError('Failed to verify password');
    } finally {
      setAuthLoading(false);
    }
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === 'Enter') handleLogin();
  }

  async function loadScans() {
    try {
      const data = await listScans(password);
      setScans(data);
    } catch {
      // silently fail on initial load
    }
  }

  async function handleSelectScan(scanId: number) {
    setLoading(true);
    setError('');
    try {
      const detail = await getScan(scanId, password);
      setSelectedScan(detail);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }

  if (!authenticated) {
    return (
      <div className="admin-login">
        <div className="admin-login-box">
          <h2>Admin Access</h2>
          <input
            type="password"
            value={passwordInput}
            onChange={e => setPasswordInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Password"
            disabled={authLoading}
            autoFocus
          />
          <button onClick={handleLogin} disabled={authLoading || !passwordInput}>
            {authLoading ? 'Checking...' : 'Submit'}
          </button>
          {authError && <div className="auth-error">{authError}</div>}
        </div>
      </div>
    );
  }

  return (
    <div className="admin-page">
      <div className="admin-layout">
        <aside className="admin-sidebar">
          <ScanHistory scans={scans} onSelect={handleSelectScan} />
        </aside>
        <main className="admin-main">
          {error && <div className="error-message">{error}</div>}
          {loading && <p className="admin-loading">Loading...</p>}
          {selectedScan && !loading && (
            <ScanResults
              formattedOutput={selectedScan.formatted_output}
              moonCount={selectedScan.moon_count}
              maxRarity={selectedScan.max_rarity}
              scanId={selectedScan.scan_id}
              scanName={selectedScan.name}
              detail={selectedScan}
            />
          )}
          {!selectedScan && !loading && !error && (
            <p className="admin-placeholder">Select a scan from the history to view details.</p>
          )}
        </main>
      </div>
    </div>
  );
}
