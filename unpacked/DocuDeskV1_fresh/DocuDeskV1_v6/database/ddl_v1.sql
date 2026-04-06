PRAGMA journal_mode = WAL;
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS documents (
    id TEXT PRIMARY KEY,
    title TEXT NOT NULL,
    original_file_name TEXT NOT NULL,
    display_name TEXT NOT NULL,
    mime_type TEXT NOT NULL,
    extension TEXT NOT NULL,
    sha256 TEXT NOT NULL,
    file_size_bytes INTEGER NOT NULL,
    page_count INTEGER NULL,
    repository_path TEXT NOT NULL,
    preview_path TEXT NULL,
    thumbnail_path TEXT NULL,
    source_type TEXT NOT NULL,
    source_reference TEXT NULL,
    language TEXT NULL,
    document_date TEXT NULL,
    sender TEXT NULL,
    recipient TEXT NULL,
    subject TEXT NULL,
    notes TEXT NULL,
    document_type TEXT NULL,
    category_id TEXT NULL,
    ocr_status TEXT NOT NULL,
    index_status TEXT NOT NULL,
    classification_status TEXT NOT NULL,
    confidence REAL NULL,
    created_at TEXT NOT NULL,
    imported_at TEXT NOT NULL,
    modified_at TEXT NOT NULL,
    is_deleted INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS document_files (
    document_id TEXT PRIMARY KEY,
    original_path TEXT NOT NULL,
    searchable_pdf_path TEXT NULL,
    thumbnail_folder TEXT NULL,
    ocr_artifact_path TEXT NULL,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS pages (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    page_number INTEGER NOT NULL,
    width_px INTEGER NULL,
    height_px INTEGER NULL,
    rotation_deg INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS document_text (
    document_id TEXT PRIMARY KEY,
    plain_text TEXT NOT NULL,
    normalized_text TEXT NULL,
    language TEXT NULL,
    ocr_confidence_avg REAL NULL,
    last_ocr_utc TEXT NULL,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS ocr_blocks (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    page_id TEXT NULL,
    block_type TEXT NOT NULL,
    text TEXT NOT NULL,
    x REAL NOT NULL,
    y REAL NOT NULL,
    width REAL NOT NULL,
    height REAL NOT NULL,
    confidence REAL NULL,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS categories (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    parent_category_id TEXT NULL,
    color TEXT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS tags (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    color TEXT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS document_tags (
    document_id TEXT NOT NULL,
    tag_id TEXT NOT NULL,
    PRIMARY KEY (document_id, tag_id),
    FOREIGN KEY(document_id) REFERENCES documents(id),
    FOREIGN KEY(tag_id) REFERENCES tags(id)
);

CREATE TABLE IF NOT EXISTS annotations (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    page_id TEXT NULL,
    annotation_type TEXT NOT NULL,
    payload_json TEXT NOT NULL,
    x REAL NULL,
    y REAL NULL,
    width REAL NULL,
    height REAL NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NULL,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS saved_searches (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    query_json TEXT NOT NULL,
    sort_mode TEXT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS tasks (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    title TEXT NOT NULL,
    due_date TEXT NULL,
    priority TEXT NOT NULL,
    status TEXT NOT NULL,
    notes TEXT NULL,
    created_at TEXT NOT NULL,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS email_links (
    id TEXT PRIMARY KEY,
    document_id TEXT NOT NULL,
    message_id TEXT NOT NULL,
    account_id TEXT NULL,
    folder_path TEXT NULL,
    attachment_name TEXT NULL,
    attachment_index INTEGER NULL,
    mail_subject TEXT NULL,
    mail_from TEXT NULL,
    mail_date TEXT NULL,
    imported_at TEXT NOT NULL,
    FOREIGN KEY(document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS duplicate_candidates (
    id TEXT PRIMARY KEY,
    document_id_a TEXT NOT NULL,
    document_id_b TEXT NOT NULL,
    match_type TEXT NOT NULL,
    score REAL NOT NULL,
    status TEXT NOT NULL,
    created_at TEXT NOT NULL,
    FOREIGN KEY(document_id_a) REFERENCES documents(id),
    FOREIGN KEY(document_id_b) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS job_records (
    id TEXT PRIMARY KEY,
    job_type TEXT NOT NULL,
    target_document_id TEXT NULL,
    payload_json TEXT NULL,
    status TEXT NOT NULL,
    error_text TEXT NULL,
    retry_count INTEGER NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL,
    started_at TEXT NULL,
    completed_at TEXT NULL,
    last_updated_at TEXT NOT NULL,
    FOREIGN KEY(target_document_id) REFERENCES documents(id)
);

CREATE TABLE IF NOT EXISTS audit_events (
    id TEXT PRIMARY KEY,
    event_type TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    created_at TEXT NOT NULL,
    payload_json TEXT NULL
);

CREATE INDEX IF NOT EXISTS idx_documents_sha256 ON documents(sha256);
CREATE INDEX IF NOT EXISTS idx_documents_document_date ON documents(document_date);
CREATE INDEX IF NOT EXISTS idx_documents_imported_at ON documents(imported_at);
CREATE INDEX IF NOT EXISTS idx_documents_category_id ON documents(category_id);
CREATE INDEX IF NOT EXISTS idx_job_records_status ON job_records(status);
CREATE INDEX IF NOT EXISTS idx_job_records_document ON job_records(target_document_id);
CREATE INDEX IF NOT EXISTS idx_email_links_message_id ON email_links(message_id);

CREATE VIRTUAL TABLE IF NOT EXISTS document_fts USING fts5(
    document_id UNINDEXED,
    title,
    sender,
    recipient,
    subject,
    notes,
    body,
    tokenize = 'unicode61 remove_diacritics 2'
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_pages_document_page ON pages(document_id, page_number);
CREATE INDEX IF NOT EXISTS idx_ocr_blocks_document_page ON ocr_blocks(document_id, page_id);

CREATE INDEX IF NOT EXISTS idx_annotations_document_page ON annotations(document_id, page_id);
