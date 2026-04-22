export interface ScanSummary {
  scan_id: number;
  name: string;
  moon_count: number;
  max_rarity: number;
  submitted_at: string;
}

export interface OreDetail {
  ore_id: number;
  ore_type: string;
  ore_type_id: number;
  percentage: number;
  rarity: number;
  sort_order: number;
}

export interface MoonDetail {
  moon_id: number;
  full_name: string;
  solar_system: string;
  planet_number: number;
  moon_number: number;
  rarity: number;
  ores: OreDetail[];
}

export interface ScanDetail {
  scan_id: number;
  name: string;
  raw_log: string;
  formatted_output: string;
  moon_count: number;
  max_rarity: number;
  submitted_at: string;
  moons: MoonDetail[];
}

export interface ScanCreateResponse {
  scan_id: number;
  name: string;
  moon_count: number;
  max_rarity: number;
  formatted_output: string;
}
