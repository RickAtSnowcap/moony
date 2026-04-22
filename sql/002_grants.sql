-- Grants for moony application user
-- Source of truth — apply via: sudo -u postgres psql -d moony < sql/002_grants.sql

GRANT USAGE ON SCHEMA moony TO moony;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA moony TO moony;
ALTER DEFAULT PRIVILEGES IN SCHEMA moony GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO moony;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA moony TO moony;
ALTER DEFAULT PRIVILEGES IN SCHEMA moony GRANT USAGE ON SEQUENCES TO moony;
