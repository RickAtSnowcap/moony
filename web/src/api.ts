import type { ScanSummary, ScanDetail, ScanCreateResponse } from './types';

const BASE = '/moony';  // app lives at /moony/, Caddy strips prefix before proxying

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

export async function listScans(password: string, limit = 20, offset = 0): Promise<ScanSummary[]> {
  const resp = await fetch(`${BASE}/api/scans?limit=${limit}&offset=${offset}`, {
    headers: { 'X-Admin-Password': password }
  });
  if (!resp.ok) throw new Error('Failed to load scans');
  return resp.json();
}

export async function getScan(id: number, password: string): Promise<ScanDetail> {
  const resp = await fetch(`${BASE}/api/scans/${id}`, {
    headers: { 'X-Admin-Password': password }
  });
  if (!resp.ok) throw new Error('Failed to load scan');
  return resp.json();
}

export async function checkPassword(password: string): Promise<boolean> {
  const resp = await fetch(`${BASE}/api/auth`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ password })
  });
  return resp.ok;
}
