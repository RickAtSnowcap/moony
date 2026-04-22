import type { ScanSummary, ScanDetail, ScanCreateResponse } from './types';

const BASE = '';  // proxied in dev, same-origin in prod

export async function submitScan(rawLog: string): Promise<ScanCreateResponse> {
  const resp = await fetch(`${BASE}/api/scans`, {
    method: 'POST',
    headers: { 'Content-Type': 'text/plain' },
    body: rawLog
  });
  if (!resp.ok) {
    const err = await resp.json();
    throw new Error(err.error || 'Failed to submit scan');
  }
  return resp.json();
}

export async function listScans(limit = 20, offset = 0): Promise<ScanSummary[]> {
  const resp = await fetch(`${BASE}/api/scans?limit=${limit}&offset=${offset}`);
  if (!resp.ok) throw new Error('Failed to load scans');
  return resp.json();
}

export async function getScan(id: number): Promise<ScanDetail> {
  const resp = await fetch(`${BASE}/api/scans/${id}`);
  if (!resp.ok) throw new Error('Failed to load scan');
  return resp.json();
}
