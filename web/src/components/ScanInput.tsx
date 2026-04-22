import { useState } from 'react';
import './ScanInput.css';

interface Props {
  onSubmit: (rawLog: string) => void;
  loading: boolean;
}

export default function ScanInput({ onSubmit, loading }: Props) {
  const [text, setText] = useState('');

  function handleSubmit() {
    if (text.trim()) onSubmit(text);
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.ctrlKey && e.key === 'Enter') handleSubmit();
  }

  return (
    <div className="scan-input">
      <label htmlFor="scan-paste">Paste moon survey scan data:</label>
      <textarea
        id="scan-paste"
        value={text}
        onChange={e => setText(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Copy from EVE Online moon survey scanner and paste here..."
        rows={10}
        disabled={loading}
      />
      <div className="scan-input-actions">
        <button onClick={handleSubmit} disabled={loading || !text.trim()}>
          {loading ? 'Processing...' : 'Format Scan'}
        </button>
        <button className="secondary" onClick={() => setText('')} disabled={loading || !text}>
          Clear
        </button>
        <span className="hint">Ctrl+Enter to submit</span>
      </div>
    </div>
  );
}
