import type { ScanSummary } from '../types';
import './ScanHistory.css';

interface Props {
  scans: ScanSummary[];
  onSelect: (scanId: number) => void;
}

function rarityClass(rarity: number): string {
  if (rarity >= 64) return 'r64';
  if (rarity >= 32) return 'r32';
  if (rarity >= 16) return 'r16';
  if (rarity >= 8) return 'r8';
  if (rarity >= 4) return 'r4';
  return 'r0';
}

function rarityLabel(rarity: number): string {
  return rarity > 0 ? `R${rarity}` : 'R0';
}

function formatDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' }) +
    ' ' + d.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });
}

export default function ScanHistory({ scans, onSelect }: Props) {
  return (
    <div className="scan-history">
      <h3>Scan History</h3>
      {scans.length === 0 && (
        <p className="no-scans">No scans yet. Paste a moon survey to get started.</p>
      )}
      {scans.map(scan => (
        <div key={scan.scan_id} className="history-item" onClick={() => onSelect(scan.scan_id)}>
          <div className="history-top">
            <span className="history-id">#{scan.scan_id}</span>
            <span className={`rarity-badge ${rarityClass(scan.max_rarity)}`}>
              {rarityLabel(scan.max_rarity)}
            </span>
          </div>
          <div className="history-meta">
            {scan.moon_count} moon{scan.moon_count !== 1 ? 's' : ''} • {formatDate(scan.submitted_at)}
          </div>
        </div>
      ))}
    </div>
  );
}
