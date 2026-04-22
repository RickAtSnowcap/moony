import { useState, useEffect } from 'react';
import type { ScanSummary, ScanDetail, ScanCreateResponse } from './types';
import { submitScan, listScans, getScan } from './api';
import ScanInput from './components/ScanInput';
import ScanResults from './components/ScanResults';
import ScanHistory from './components/ScanHistory';
import './App.css';

function App() {
  const [scans, setScans] = useState<ScanSummary[]>([]);
  const [currentResult, setCurrentResult] = useState<ScanCreateResponse | null>(null);
  const [selectedScan, setSelectedScan] = useState<ScanDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadScans();
  }, []);

  async function loadScans() {
    try {
      const data = await listScans();
      setScans(data);
    } catch {
      // silently fail on initial load
    }
  }

  async function handleSubmit(rawLog: string) {
    setLoading(true);
    setError('');
    setSelectedScan(null);
    try {
      const result = await submitScan(rawLog);
      setCurrentResult(result);
      await loadScans();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }

  async function handleSelectScan(scanId: number) {
    setLoading(true);
    setError('');
    setCurrentResult(null);
    try {
      const detail = await getScan(scanId);
      setSelectedScan(detail);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>🌑 Moony</h1>
        <span className="subtitle">EVE Online Moon Scan Formatter</span>
      </header>
      <div className="app-layout">
        <main className="app-main">
          <ScanInput onSubmit={handleSubmit} loading={loading} />
          {error && <div className="error-message">{error}</div>}
          {currentResult && (
            <ScanResults
              formattedOutput={currentResult.formatted_output}
              moonCount={currentResult.moon_count}
              maxRarity={currentResult.max_rarity}
              scanId={currentResult.scan_id}
              onViewDetail={() => handleSelectScan(currentResult.scan_id)}
            />
          )}
          {selectedScan && (
            <ScanResults
              formattedOutput={selectedScan.formatted_output}
              moonCount={selectedScan.moon_count}
              maxRarity={selectedScan.max_rarity}
              scanId={selectedScan.scan_id}
              detail={selectedScan}
            />
          )}
        </main>
        <aside className="app-sidebar">
          <ScanHistory scans={scans} onSelect={handleSelectScan} />
        </aside>
      </div>
    </div>
  );
}

export default App;
