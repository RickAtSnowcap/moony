-- Add name column to scans table and update insert function
-- Source of truth — apply via: cat sql/004_scan_name.sql | sudo -u postgres psql -d moony

-- Add name column
ALTER TABLE moony.scans ADD COLUMN IF NOT EXISTS name TEXT NOT NULL DEFAULT '';

-- Drop old function signatures whose return types change
DROP FUNCTION IF EXISTS moony.fn_scan_insert(text, text, integer, integer);
DROP FUNCTION IF EXISTS moony.fn_scan_list(integer, integer);
DROP FUNCTION IF EXISTS moony.fn_scan_get(bigint);

-- Replace fn_scan_insert to accept name
CREATE OR REPLACE FUNCTION moony.fn_scan_insert(
    p_raw_log TEXT,
    p_formatted_output TEXT,
    p_moon_count INT,
    p_max_rarity INT,
    p_name TEXT DEFAULT ''
) RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO moony.scans (raw_log, formatted_output, moon_count, max_rarity, name)
    VALUES (p_raw_log, p_formatted_output, p_moon_count, p_max_rarity, p_name)
    RETURNING scan_id;
$$;

-- Function to count existing scans with same system prefix for sequence numbering
CREATE OR REPLACE FUNCTION moony.fn_scan_count_by_system(
    p_system TEXT
) RETURNS INT
LANGUAGE sql AS $$
    SELECT COUNT(*)::INT FROM moony.scans WHERE name LIKE p_system || ' #%';
$$;

-- Update fn_scan_list to include name
CREATE OR REPLACE FUNCTION moony.fn_scan_list(
    p_limit INT DEFAULT 20,
    p_offset INT DEFAULT 0
) RETURNS TABLE(
    scan_id BIGINT,
    name TEXT,
    moon_count INT,
    max_rarity INT,
    submitted_at TIMESTAMPTZ
)
LANGUAGE sql AS $$
    SELECT s.scan_id, s.name, s.moon_count, s.max_rarity, s.submitted_at
    FROM moony.scans s
    ORDER BY s.submitted_at DESC
    LIMIT p_limit OFFSET p_offset;
$$;

-- Update fn_scan_get to include name
CREATE OR REPLACE FUNCTION moony.fn_scan_get(
    p_scan_id BIGINT
) RETURNS TABLE(
    scan_id BIGINT,
    name TEXT,
    raw_log TEXT,
    formatted_output TEXT,
    moon_count INT,
    max_rarity INT,
    submitted_at TIMESTAMPTZ
)
LANGUAGE sql AS $$
    SELECT s.scan_id, s.name, s.raw_log, s.formatted_output, s.moon_count, s.max_rarity, s.submitted_at
    FROM moony.scans s
    WHERE s.scan_id = p_scan_id;
$$;

GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA moony TO moony;
