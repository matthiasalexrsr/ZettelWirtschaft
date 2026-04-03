PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS __schema_migrations (
    version TEXT PRIMARY KEY,
    applied_utc TEXT NOT NULL
);

CREATE TABLE categories (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    parent_category_id TEXT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0,
    is_system INTEGER NOT NULL DEFAULT 0 CHECK (is_system IN (0, 1)),
    created_utc TEXT NOT NULL,
    FOREIGN KEY (parent_category_id) REFERENCES categories(id) ON DELETE SET NULL
);

CREATE UNIQUE INDEX idx_categories_name ON categories(name);
CREATE INDEX idx_categories_parent ON categories(parent_category_id);
CREATE INDEX idx_categories_sort ON categories(sort_order);

CREATE TABLE tags (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    color TEXT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_utc TEXT NOT NULL
);

CREATE UNIQUE INDEX idx_tags_name ON tags(name);
CREATE INDEX idx_tags_sort ON tags(sort_order);

CREATE TABLE documents (
    id TEXT PRIMARY KEY NOT NULL,
    title TEXT NULL,
    original_file_name TEXT NOT NULL,
    mime_type TEXT NOT NULL,
    sha256 TEXT NOT NULL,
    page_count INTEGER NOT NULL DEFAULT 0 CHECK (page_count >= 0),
    import_date_utc TEXT NOT NULL,
    document_date TEXT NULL,
    status TEXT NOT NULL DEFAULT 'new'
        CHECK (status IN ('new', 'processing', 'ready', 'review', 'error', 'archived')),
    sender TEXT NULL,
    recipient TEXT NULL,
    notes TEXT NULL,
    category_id TEXT NULL,
    has_ocr INTEGER NOT NULL DEFAULT 0 CHECK (has_ocr IN (0, 1)),
    is_deleted INTEGER NOT NULL DEFAULT 0 CHECK (is_deleted IN (0, 1)),
    last_modified_utc TEXT NOT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE SET NULL
);

CREATE UNIQUE INDEX idx_documents_sha256 ON documents(sha256);
CREATE INDEX idx_documents_category ON documents(category_id);
CREATE INDEX idx_documents_status ON documents(status);
CREATE INDEX idx_documents_import_date ON documents(import_date_utc DESC);
CREATE INDEX idx_documents_document_date ON documents(document_date DESC);
CREATE INDEX idx_documents_title ON documents(title);

CREATE TABLE document_files (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    document_id TEXT NOT NULL UNIQUE,
    original_path TEXT NOT NULL,
    searchable_pdf_path TEXT NULL,
    thumbnail_folder TEXT NULL,
    ocr_artifact_path TEXT NULL,
    file_size_bytes INTEGER NOT NULL CHECK (file_size_bytes >= 0),
    storage_state TEXT NOT NULL DEFAULT 'present'
        CHECK (storage_state IN ('present', 'missing', 'orphaned', 'restored')),
    created_utc TEXT NOT NULL,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE INDEX idx_document_files_document ON document_files(document_id);

CREATE TABLE document_pages (
    id TEXT PRIMARY KEY NOT NULL,
    document_id TEXT NOT NULL,
    page_number INTEGER NOT NULL CHECK (page_number >= 1),
    width_px INTEGER NULL CHECK (width_px IS NULL OR width_px >= 0),
    height_px INTEGER NULL CHECK (height_px IS NULL OR height_px >= 0),
    rotation_deg INTEGER NOT NULL DEFAULT 0 CHECK (rotation_deg IN (0, 90, 180, 270)),
    preview_image_path TEXT NULL,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE,
    UNIQUE (document_id, page_number)
);

CREATE INDEX idx_document_pages_document ON document_pages(document_id);
CREATE INDEX idx_document_pages_order ON document_pages(document_id, page_number);

CREATE TABLE document_text (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    document_id TEXT NOT NULL UNIQUE,
    plain_text TEXT NOT NULL DEFAULT '',
    normalized_text TEXT NOT NULL DEFAULT '',
    language TEXT NULL,
    ocr_confidence_avg REAL NULL CHECK (ocr_confidence_avg IS NULL OR (ocr_confidence_avg >= 0 AND ocr_confidence_avg <= 100)),
    last_ocr_utc TEXT NULL,
    row_version INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
);

CREATE INDEX idx_document_text_document ON document_text(document_id);

CREATE TABLE document_tags (
    document_id TEXT NOT NULL,
    tag_id TEXT NOT NULL,
    created_utc TEXT NOT NULL,
    PRIMARY KEY (document_id, tag_id),
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES tags(id) ON DELETE CASCADE
);

CREATE INDEX idx_document_tags_tag ON document_tags(tag_id);
CREATE INDEX idx_document_tags_document ON document_tags(document_id);
