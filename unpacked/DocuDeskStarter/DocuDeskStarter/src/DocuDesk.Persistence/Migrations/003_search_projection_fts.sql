PRAGMA foreign_keys = ON;

CREATE TABLE search_documents (
    document_id TEXT PRIMARY KEY NOT NULL,
    title TEXT NOT NULL DEFAULT '',
    sender TEXT NOT NULL DEFAULT '',
    recipient TEXT NOT NULL DEFAULT '',
    notes TEXT NOT NULL DEFAULT '',
    plain_text TEXT NOT NULL DEFAULT '',
    updated_utc TEXT NOT NULL,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE VIRTUAL TABLE fts_search_documents USING fts5(
    document_id UNINDEXED,
    title,
    sender,
    recipient,
    notes,
    plain_text,
    content='search_documents',
    content_rowid='rowid',
    tokenize='unicode61 remove_diacritics 2'
);

CREATE TRIGGER trg_search_documents_ai
AFTER INSERT ON search_documents
BEGIN
    INSERT INTO fts_search_documents(rowid, document_id, title, sender, recipient, notes, plain_text)
    VALUES (NEW.rowid, NEW.document_id, COALESCE(NEW.title, ''), COALESCE(NEW.sender, ''), COALESCE(NEW.recipient, ''), COALESCE(NEW.notes, ''), COALESCE(NEW.plain_text, ''));
END;

CREATE TRIGGER trg_search_documents_au
AFTER UPDATE ON search_documents
BEGIN
    INSERT INTO fts_search_documents(fts_search_documents, rowid, document_id, title, sender, recipient, notes, plain_text)
    VALUES('delete', OLD.rowid, OLD.document_id, OLD.title, OLD.sender, OLD.recipient, OLD.notes, OLD.plain_text);

    INSERT INTO fts_search_documents(rowid, document_id, title, sender, recipient, notes, plain_text)
    VALUES (NEW.rowid, NEW.document_id, COALESCE(NEW.title, ''), COALESCE(NEW.sender, ''), COALESCE(NEW.recipient, ''), COALESCE(NEW.notes, ''), COALESCE(NEW.plain_text, ''));
END;

CREATE TRIGGER trg_search_documents_ad
AFTER DELETE ON search_documents
BEGIN
    INSERT INTO fts_search_documents(fts_search_documents, rowid, document_id, title, sender, recipient, notes, plain_text)
    VALUES('delete', OLD.rowid, OLD.document_id, OLD.title, OLD.sender, OLD.recipient, OLD.notes, OLD.plain_text);
END;

CREATE TRIGGER trg_documents_ai_search
AFTER INSERT ON documents
BEGIN
    INSERT INTO search_documents(document_id, title, sender, recipient, notes, plain_text, updated_utc)
    VALUES (NEW.id, COALESCE(NEW.title, ''), COALESCE(NEW.sender, ''), COALESCE(NEW.recipient, ''), COALESCE(NEW.notes, ''), '', COALESCE(NEW.last_modified_utc, NEW.import_date_utc));
END;

CREATE TRIGGER trg_documents_au_search
AFTER UPDATE OF title, sender, recipient, notes, last_modified_utc ON documents
BEGIN
    UPDATE search_documents
       SET title = COALESCE(NEW.title, ''),
           sender = COALESCE(NEW.sender, ''),
           recipient = COALESCE(NEW.recipient, ''),
           notes = COALESCE(NEW.notes, ''),
           updated_utc = COALESCE(NEW.last_modified_utc, CURRENT_TIMESTAMP)
     WHERE document_id = NEW.id;
END;

CREATE TRIGGER trg_documents_ad_search
AFTER DELETE ON documents
BEGIN
    DELETE FROM search_documents WHERE document_id = OLD.id;
END;

CREATE TRIGGER trg_document_text_ai_search
AFTER INSERT ON document_text
BEGIN
    INSERT INTO search_documents(document_id, plain_text, updated_utc)
    VALUES (NEW.document_id, COALESCE(NULLIF(NEW.normalized_text, ''), NEW.plain_text, ''), COALESCE(NEW.last_ocr_utc, CURRENT_TIMESTAMP))
    ON CONFLICT(document_id) DO UPDATE SET
        plain_text = COALESCE(NULLIF(NEW.normalized_text, ''), NEW.plain_text, ''),
        updated_utc = COALESCE(NEW.last_ocr_utc, CURRENT_TIMESTAMP);
END;

CREATE TRIGGER trg_document_text_au_search
AFTER UPDATE OF plain_text, normalized_text, last_ocr_utc ON document_text
BEGIN
    UPDATE search_documents
       SET plain_text = COALESCE(NULLIF(NEW.normalized_text, ''), NEW.plain_text, ''),
           updated_utc = COALESCE(NEW.last_ocr_utc, CURRENT_TIMESTAMP)
     WHERE document_id = NEW.document_id;
END;

CREATE TRIGGER trg_document_text_ad_search
AFTER DELETE ON document_text
BEGIN
    UPDATE search_documents
       SET plain_text = '',
           updated_utc = CURRENT_TIMESTAMP
     WHERE document_id = OLD.document_id;
END;

CREATE TABLE saved_searches (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    query_json TEXT NOT NULL,
    sort_mode TEXT NOT NULL DEFAULT 'relevance',
    is_pinned INTEGER NOT NULL DEFAULT 0 CHECK (is_pinned IN (0, 1)),
    created_utc TEXT NOT NULL,
    updated_utc TEXT NOT NULL
);

CREATE UNIQUE INDEX idx_saved_searches_name ON saved_searches(name);
CREATE INDEX idx_saved_searches_pinned ON saved_searches(is_pinned, updated_utc DESC);

INSERT INTO search_documents(document_id, title, sender, recipient, notes, plain_text, updated_utc)
SELECT
    d.id,
    COALESCE(d.title, ''),
    COALESCE(d.sender, ''),
    COALESCE(d.recipient, ''),
    COALESCE(d.notes, ''),
    COALESCE(NULLIF(dt.normalized_text, ''), dt.plain_text, ''),
    COALESCE(dt.last_ocr_utc, d.last_modified_utc, d.import_date_utc, CURRENT_TIMESTAMP)
FROM documents d
LEFT JOIN document_text dt ON dt.document_id = d.id
ON CONFLICT(document_id) DO UPDATE SET
    title = excluded.title,
    sender = excluded.sender,
    recipient = excluded.recipient,
    notes = excluded.notes,
    plain_text = excluded.plain_text,
    updated_utc = excluded.updated_utc;
