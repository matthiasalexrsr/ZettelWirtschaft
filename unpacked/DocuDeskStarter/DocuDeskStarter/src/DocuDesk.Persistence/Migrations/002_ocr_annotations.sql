PRAGMA foreign_keys = ON;

CREATE TABLE ocr_blocks (
    id TEXT PRIMARY KEY NOT NULL,
    document_id TEXT NOT NULL,
    page_id TEXT NOT NULL,
    parent_block_id TEXT NULL,
    block_type TEXT NOT NULL CHECK (block_type IN ('page', 'block', 'paragraph', 'line', 'word')),
    text TEXT NOT NULL DEFAULT '',
    x_norm REAL NOT NULL CHECK (x_norm >= 0 AND x_norm <= 1),
    y_norm REAL NOT NULL CHECK (y_norm >= 0 AND y_norm <= 1),
    w_norm REAL NOT NULL CHECK (w_norm >= 0 AND w_norm <= 1),
    h_norm REAL NOT NULL CHECK (h_norm >= 0 AND h_norm <= 1),
    confidence REAL NULL CHECK (confidence IS NULL OR (confidence >= 0 AND confidence <= 100)),
    reading_order INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE,
    FOREIGN KEY (page_id) REFERENCES document_pages(id) ON DELETE CASCADE,
    FOREIGN KEY (parent_block_id) REFERENCES ocr_blocks(id) ON DELETE CASCADE
);

CREATE INDEX idx_ocr_blocks_document ON ocr_blocks(document_id);
CREATE INDEX idx_ocr_blocks_page ON ocr_blocks(page_id);
CREATE INDEX idx_ocr_blocks_page_order ON ocr_blocks(page_id, reading_order);
CREATE INDEX idx_ocr_blocks_type ON ocr_blocks(block_type);
CREATE INDEX idx_ocr_blocks_parent ON ocr_blocks(parent_block_id);

CREATE TABLE annotations (
    id TEXT PRIMARY KEY NOT NULL,
    document_id TEXT NOT NULL,
    page_id TEXT NOT NULL,
    type TEXT NOT NULL CHECK (type IN ('highlight', 'rectangle', 'arrow', 'note', 'stamp', 'redaction')),
    payload_json TEXT NOT NULL DEFAULT '{}',
    x_norm REAL NOT NULL CHECK (x_norm >= 0 AND x_norm <= 1),
    y_norm REAL NOT NULL CHECK (y_norm >= 0 AND y_norm <= 1),
    w_norm REAL NOT NULL CHECK (w_norm >= 0 AND w_norm <= 1),
    h_norm REAL NOT NULL CHECK (h_norm >= 0 AND h_norm <= 1),
    z_index INTEGER NOT NULL DEFAULT 0,
    author_name TEXT NULL,
    created_utc TEXT NOT NULL,
    updated_utc TEXT NOT NULL,
    FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE,
    FOREIGN KEY (page_id) REFERENCES document_pages(id) ON DELETE CASCADE
);

CREATE INDEX idx_annotations_document ON annotations(document_id);
CREATE INDEX idx_annotations_page ON annotations(page_id);
CREATE INDEX idx_annotations_page_type ON annotations(page_id, type);
CREATE INDEX idx_annotations_updated ON annotations(updated_utc DESC);
