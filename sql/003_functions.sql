-- Stored functions for Moony API
-- Source of truth — apply via: cat sql/003_functions.sql | sudo -u postgres psql -d moony

-- Insert a scan, return the new scan_id
CREATE OR REPLACE FUNCTION moony.fn_scan_insert(
    p_raw_log TEXT,
    p_formatted_output TEXT,
    p_moon_count INT,
    p_max_rarity INT
) RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO moony.scans (raw_log, formatted_output, moon_count, max_rarity)
    VALUES (p_raw_log, p_formatted_output, p_moon_count, p_max_rarity)
    RETURNING scan_id;
$$;

-- Insert a moon, return the new moon_id
CREATE OR REPLACE FUNCTION moony.fn_moon_insert(
    p_scan_id BIGINT,
    p_full_name TEXT,
    p_solar_system TEXT,
    p_planet_number INT,
    p_moon_number INT,
    p_rarity INT,
    p_solar_system_id BIGINT DEFAULT NULL,
    p_planet_id BIGINT DEFAULT NULL,
    p_eve_moon_id BIGINT DEFAULT NULL
) RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO moony.moons (scan_id, full_name, solar_system, planet_number, moon_number, rarity, solar_system_id, planet_id, eve_moon_id)
    VALUES (p_scan_id, p_full_name, p_solar_system, p_planet_number, p_moon_number, p_rarity, p_solar_system_id, p_planet_id, p_eve_moon_id)
    RETURNING moon_id;
$$;

-- Insert an ore, return the new ore_id
CREATE OR REPLACE FUNCTION moony.fn_ore_insert(
    p_moon_id BIGINT,
    p_ore_type TEXT,
    p_ore_type_id INT,
    p_percentage INT,
    p_rarity INT,
    p_sort_order INT
) RETURNS BIGINT
LANGUAGE sql AS $$
    INSERT INTO moony.ores (moon_id, ore_type, ore_type_id, percentage, rarity, sort_order)
    VALUES (p_moon_id, p_ore_type, p_ore_type_id, p_percentage, p_rarity, p_sort_order)
    RETURNING ore_id;
$$;

-- List scans with summary info, paginated
CREATE OR REPLACE FUNCTION moony.fn_scan_list(
    p_limit INT DEFAULT 20,
    p_offset INT DEFAULT 0
) RETURNS TABLE(
    scan_id BIGINT,
    moon_count INT,
    max_rarity INT,
    submitted_at TIMESTAMPTZ
)
LANGUAGE sql AS $$
    SELECT s.scan_id, s.moon_count, s.max_rarity, s.submitted_at
    FROM moony.scans s
    ORDER BY s.submitted_at DESC
    LIMIT p_limit OFFSET p_offset;
$$;

-- Get a single scan header
CREATE OR REPLACE FUNCTION moony.fn_scan_get(
    p_scan_id BIGINT
) RETURNS TABLE(
    scan_id BIGINT,
    raw_log TEXT,
    formatted_output TEXT,
    moon_count INT,
    max_rarity INT,
    submitted_at TIMESTAMPTZ
)
LANGUAGE sql AS $$
    SELECT s.scan_id, s.raw_log, s.formatted_output, s.moon_count, s.max_rarity, s.submitted_at
    FROM moony.scans s
    WHERE s.scan_id = p_scan_id;
$$;

-- Get moons for a scan
CREATE OR REPLACE FUNCTION moony.fn_moons_by_scan(
    p_scan_id BIGINT
) RETURNS TABLE(
    moon_id BIGINT,
    full_name TEXT,
    solar_system TEXT,
    planet_number INT,
    moon_number INT,
    rarity INT,
    solar_system_id BIGINT,
    planet_id BIGINT,
    eve_moon_id BIGINT
)
LANGUAGE sql AS $$
    SELECT m.moon_id, m.full_name, m.solar_system, m.planet_number, m.moon_number, m.rarity, m.solar_system_id, m.planet_id, m.eve_moon_id
    FROM moony.moons m
    WHERE m.scan_id = p_scan_id
    ORDER BY m.solar_system, m.planet_number, m.moon_number;
$$;

-- Get ores for a moon
CREATE OR REPLACE FUNCTION moony.fn_ores_by_moon(
    p_moon_id BIGINT
) RETURNS TABLE(
    ore_id BIGINT,
    ore_type TEXT,
    ore_type_id INT,
    percentage INT,
    rarity INT,
    sort_order INT
)
LANGUAGE sql AS $$
    SELECT o.ore_id, o.ore_type, o.ore_type_id, o.percentage, o.rarity, o.sort_order
    FROM moony.ores o
    WHERE o.moon_id = p_moon_id
    ORDER BY o.sort_order;
$$;

-- Grants
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA moony TO moony;
ALTER DEFAULT PRIVILEGES IN SCHEMA moony GRANT EXECUTE ON FUNCTIONS TO moony;
