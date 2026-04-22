-- Moony schema and tables
-- Source of truth — apply via: sudo -u postgres psql -d moony < sql/001_schema.sql

CREATE SCHEMA IF NOT EXISTS moony;

CREATE TABLE moony.scans (
    scan_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    raw_log TEXT NOT NULL,
    formatted_output TEXT NOT NULL,
    moon_count INT NOT NULL,
    max_rarity INT NOT NULL,
    submitted_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE moony.moons (
    moon_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    scan_id BIGINT NOT NULL REFERENCES moony.scans(scan_id),
    full_name TEXT NOT NULL,
    solar_system TEXT NOT NULL,
    planet_number INT NOT NULL,
    moon_number INT NOT NULL,
    rarity INT NOT NULL,
    solar_system_id BIGINT,
    planet_id BIGINT,
    eve_moon_id BIGINT
);

CREATE TABLE moony.ores (
    ore_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    moon_id BIGINT NOT NULL REFERENCES moony.moons(moon_id),
    ore_type TEXT NOT NULL,
    ore_type_id INT NOT NULL,
    percentage INT NOT NULL,
    rarity INT NOT NULL,
    sort_order INT NOT NULL
);
