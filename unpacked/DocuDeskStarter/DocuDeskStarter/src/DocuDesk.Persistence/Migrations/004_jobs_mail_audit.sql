PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS background_jobs (
    id TEXT PRIMARY KEY NOT NULL,
    job_type TEXT NOT NULL
        CHECK (job_type IN ('import', 'thumbnail', 'ocr', 'reindex', 'export', 'integrity_check')),
    related_document_id TEXT NULL,
    payload_json TEXT NOT NULL DEFAULT '{}',
    state TEXT NOT NULL DEFAULT 'queued'
        CHECK (state IN ('queued', 'running', 'completed', 'failed', 'cancelled')),
    priority INTEGER NOT NULL DEFAULT 100,
    attempts INTEGER NOT NULL DEFAULT 0,
    max_attempts INTEGER NOT NULL DEFAULT 3,
    scheduled_utc TEXT NOT NULL,
    started_utc TEXT NULL,
    completed_utc TEXT NULL,
    error_text TEXT NULL,
    created_utc TEXT NOT NULL,
    FOREIGN KEY (related_document_id) REFERENCES documents(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_background_jobs_state_priority
    ON background_jobs(state, priority, scheduled_utc);

CREATE INDEX IF NOT EXISTS idx_background_jobs_document
    ON background_jobs(related_document_id);

CREATE TABLE IF NOT EXISTS mail_jobs (
    id TEXT PRIMARY KEY NOT NULL,
    target_client TEXT NOT NULL
        CHECK (target_client IN ('thunderbird')),
    direction TEXT NOT NULL
        CHECK (direction IN ('app_to_mail', 'mail_to_app')),
    request_json TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'queued'
        CHECK (status IN ('queued', 'dispatched', 'completed', 'failed')),
    external_reference TEXT NULL,
    error_text TEXT NULL,
    created_utc TEXT NOT NULL,
    completed_utc TEXT NULL
);

CREATE INDEX IF NOT EXISTS idx_mail_jobs_status
    ON mail_jobs(status, created_utc DESC);

CREATE TABLE IF NOT EXISTS mail_job_documents (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    mail_job_id TEXT NOT NULL,
    document_id TEXT NOT NULL,
    export_artifact_path TEXT NULL,
    attachment_role TEXT NOT NULL DEFAULT 'original'
        CHECK (attachment_role IN ('original', 'searchable_pdf', 'extract_text', 'image_clip')),
    FOREIGN KEY (mail_job_id) REFERENCES mail_jobs(id) ON DELETE CASCADE,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_mail_job_documents_job
    ON mail_job_documents(mail_job_id);

CREATE INDEX IF NOT EXISTS idx_mail_job_documents_document
    ON mail_job_documents(document_id);

CREATE TABLE IF NOT EXISTS audit_events (
    id TEXT PRIMARY KEY NOT NULL,
    event_type TEXT NOT NULL,
    entity_type TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    document_id TEXT NULL,
    payload_json TEXT NOT NULL DEFAULT '{}',
    created_utc TEXT NOT NULL,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_audit_events_document
    ON audit_events(document_id, created_utc DESC);

CREATE INDEX IF NOT EXISTS idx_audit_events_entity
    ON audit_events(entity_type, entity_id, created_utc DESC);

CREATE INDEX IF NOT EXISTS idx_audit_events_type
    ON audit_events(event_type, created_utc DESC);
