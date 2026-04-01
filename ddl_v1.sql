PRAGMA journal_mode = WAL;
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS documents (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL,
    original_file_name TEXT NOT NULL,
    mime_type TEXT NOT NULL,
    sha256 TEXT NOT NULL,
    page_count INTEGER NOT NULL DEFAULT 0,
    import_date_utc TEXT NOT NULL,
    document_date TEXT NULL,
    status TEXT NOT NULL,
    category_id TEXT NULL,
    sender TEXT NULL,
    recipient TEXT NULL,
    notes TEXT NULL,
    is_deleted INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS document_files (
    document_id TEXT PRIMARY KEY,
    original_path TEXT NOT NULL,
    searchable_pdf_path TEXT NULL,
    thumbnail_folder TEXT NULL,
    ocr_artifact_path TEXT NULL,
    file_size_bytes INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS pages (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    page_number INTEGER NOT NULL,
    width_px INTEGER NOT NULL DEFAULT 0,
    height_px INTEGER NOT NULL DEFAULT 0,
    rotation_deg INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS document_text (
    document_id TEXT PRIMARY KEY,
    plain_text TEXT NOT NULL,
    normalized_text TEXT NOT NULL,
    language TEXT NULL,
    ocr_confidence_avg REAL NULL,
    last_ocr_utc TEXT NULL
);

CREATE TABLE IF NOT EXISTS ocr_blocks (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    page_id TEXT NOT NULL,
    block_type TEXT NOT NULL,
    text TEXT NOT NULL,
    x REAL NOT NULL,
    y REAL NOT NULL,
    width REAL NOT NULL,
    height REAL NOT NULL,
    confidence REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS tags (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    color TEXT NULL
);

CREATE TABLE IF NOT EXISTS document_tags (
    document_id TEXT NOT NULL,
    tag_id TEXT NOT NULL,
    PRIMARY KEY (document_id, tag_id)
);

CREATE TABLE IF NOT EXISTS categories (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    parent_category_id TEXT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS annotations (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    page_id TEXT NULL,
    type TEXT NOT NULL,
    payload_json TEXT NOT NULL,
    x REAL NOT NULL DEFAULT 0,
    y REAL NOT NULL DEFAULT 0,
    width REAL NOT NULL DEFAULT 0,
    height REAL NOT NULL DEFAULT 0,
    created_utc TEXT NOT NULL,
    updated_utc TEXT NULL
);

CREATE TABLE IF NOT EXISTS saved_searches (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    query_json TEXT NOT NULL,
    sort_mode TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS tasks (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    title TEXT NOT NULL,
    due_date_utc TEXT NULL,
    priority TEXT NOT NULL,
    status TEXT NOT NULL,
    notes TEXT NULL
);

CREATE TABLE IF NOT EXISTS email_links (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    message_id TEXT NOT NULL,
    account_id TEXT NULL,
    folder_path TEXT NULL,
    attachment_name TEXT NULL,
    mail_subject TEXT NULL,
    mail_from TEXT NULL,
    mail_date_utc TEXT NULL
);

CREATE TABLE IF NOT EXISTS duplicate_candidates (
    id TEXT PRIMARY KEY,
    document_id_a TEXT NOT NULL,
    document_id_b TEXT NOT NULL,
    match_type TEXT NOT NULL,
    score REAL NOT NULL,
    status TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS job_records (
    id TEXT PRIMARY KEY,
    job_type TEXT NOT NULL,
    status TEXT NOT NULL,
    payload_json TEXT NOT NULL,
    error_text TEXT NULL,
    created_utc TEXT NOT NULL,
    started_utc TEXT NULL,
    completed_utc TEXT NULL
);

CREATE TABLE IF NOT EXISTS audit_events (
    id TEXT PRIMARY KEY,
    event_type TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    created_utc TEXT NOT NULL,
    payload_json TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_documents_sha256 ON documents (sha256);
CREATE INDEX IF NOT EXISTS idx_documents_document_date ON documents (document_date);
CREATE INDEX IF NOT EXISTS idx_documents_import_date_utc ON documents (import_date_utc);
CREATE INDEX IF NOT EXISTS idx_annotations_document_id ON annotations (document_id);
CREATE INDEX IF NOT EXISTS idx_email_links_message_id ON email_links (message_id);
CREATE INDEX IF NOT EXISTS idx_job_records_status ON job_records (status);

CREATE VIRTUAL TABLE IF NOT EXISTS fts_document_text USING fts5(
    document_id UNINDEXED,
    title,
    sender,
    recipient,
    notes,
    plain_text,
    tokenize = 'unicode61 remove_diacritics 2'
);
