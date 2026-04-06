PRAGMA foreign_keys = ON;

INSERT INTO categories (id, name, parent_category_id, sort_order, is_system, created_utc)
VALUES
    ('cat-inbox', 'Inbox', NULL, 0, 1, CURRENT_TIMESTAMP),
    ('cat-correspondence', 'Korrespondenz', NULL, 10, 1, CURRENT_TIMESTAMP),
    ('cat-reports', 'Befunde', NULL, 20, 1, CURRENT_TIMESTAMP),
    ('cat-billing', 'Abrechnung', NULL, 30, 1, CURRENT_TIMESTAMP),
    ('cat-archive', 'Archiv', NULL, 999, 1, CURRENT_TIMESTAMP)
ON CONFLICT(id) DO NOTHING;

INSERT INTO tags (id, name, color, sort_order, created_utc)
VALUES
    ('tag-needs-review', 'Zu prüfen', '#E67E22', 10, CURRENT_TIMESTAMP),
    ('tag-done', 'Erledigt', '#27AE60', 20, CURRENT_TIMESTAMP),
    ('tag-follow-up', 'Wiedervorlage', '#2980B9', 30, CURRENT_TIMESTAMP),
    ('tag-mail', 'Per Mail bearbeiten', '#8E44AD', 40, CURRENT_TIMESTAMP)
ON CONFLICT(id) DO NOTHING;
