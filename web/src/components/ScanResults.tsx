import { useState } from 'react';
import type { ScanDetail } from '../types';
import { copyText } from '../copyText';
import './ScanResults.css';

interface Props {
  formattedOutput: string;
  moonCount: number;
  maxRarity: number;
  scanId: number;
  scanName?: string;
  detail?: ScanDetail;
  onViewDetail?: () => void;
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

export default function ScanResults({ formattedOutput, moonCount, maxRarity, scanId, scanName, detail, onViewDetail }: Props) {
  const [copied, setCopied] = useState(false);
  const [expandedMoons, setExpandedMoons] = useState<Set<number>>(new Set());

  function handleCopy() {
    copyText(formattedOutput);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  function toggleMoon(moonId: number) {
    setExpandedMoons(prev => {
      const next = new Set(prev);
      if (next.has(moonId)) next.delete(moonId);
      else next.add(moonId);
      return next;
    });
  }

  return (
    <div className="scan-results">
      <div className="results-header">
        <h2>{scanName || `Scan #${scanId}`}</h2>
        <span className="results-meta">
          {moonCount} moon{moonCount !== 1 ? 's' : ''} •
          Max: <span className={`rarity ${rarityClass(maxRarity)}`}>{rarityLabel(maxRarity)}</span>
        </span>
      </div>

      <div className="formatted-section">
        <div className="section-header">
          <h3>Formatted Output</h3>
          <button className="copy-btn" onClick={handleCopy}>
            {copied ? '✓ Copied' : 'Copy'}
          </button>
        </div>
        <textarea className="formatted-output" readOnly value={formattedOutput} rows={Math.min(formattedOutput.split('\n').length + 1, 15)} />
      </div>

      {detail && detail.moons.length > 0 && (
        <div className="detail-section">
          <h3>Moon Details</h3>
          <div className="moon-table">
            {detail.moons.map(moon => (
              <div key={moon.moon_id} className="moon-row">
                <div className="moon-header" onClick={() => toggleMoon(moon.moon_id)}>
                  <span className="expand-icon">{expandedMoons.has(moon.moon_id) ? '▼' : '▶'}</span>
                  <span className="moon-name">{moon.full_name}</span>
                  <span className={`rarity-badge ${rarityClass(moon.rarity)}`}>{rarityLabel(moon.rarity)}</span>
                </div>
                {expandedMoons.has(moon.moon_id) && (
                  <div className="ore-list">
                    {moon.ores.map(ore => (
                      <div key={ore.ore_id} className="ore-row">
                        <span className={`ore-name ${rarityClass(ore.rarity)}`}>{ore.ore_type}</span>
                        <span className="ore-pct">{ore.percentage}%</span>
                        <span className={`ore-rarity ${rarityClass(ore.rarity)}`}>{rarityLabel(ore.rarity)}</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {onViewDetail && !detail && (
        <button className="view-detail-btn" onClick={onViewDetail}>View Full Details</button>
      )}
    </div>
  );
}
