SELECT 'CREATE DATABASE kidbank_settings'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'kidbank_settings')\gexec
