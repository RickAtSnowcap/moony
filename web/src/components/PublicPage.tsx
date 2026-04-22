import { useState } from 'react';
import type { ScanCreateResponse } from '../types';
import { submitScan } from '../api';
import ScanInput from './ScanInput';
import './PublicPage.css';

export default function PublicPage() {
  const [result, setResult] = useState<ScanCreateResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [copied, setCopied] = useState(false);

  async function handleSubmit(rawLog: string) {
    setLoading(true);
    setError('');
    setResult(null);
    try {
      const data = await submitScan(rawLog);
      setResult(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  }

  function handleCopy() {
    if (!result) return;
    navigator.clipboard.writeText(result.formatted_output);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  return (
    <div className="public-page">
      <ScanInput onSubmit={handleSubmit} loading={loading} />
      {error && <div className="error-message">{error}</div>}
      {result && (
        <div className="public-result">
          <div className="public-result-header">
            <span className="public-result-meta">
              {result.moon_count} moon{result.moon_count !== 1 ? 's' : ''} formatted
            </span>
            <button className="copy-btn" onClick={handleCopy}>
              {copied ? '✓ Copied' : 'Copy'}
            </button>
          </div>
          <textarea
            className="formatted-output"
            readOnly
            value={result.formatted_output}
            rows={Math.min(result.formatted_output.split('\n').length + 1, 20)}
          />
        </div>
      )}
    </div>
  );
}
