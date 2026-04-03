from __future__ import annotations

import argparse
import hashlib
import mimetypes
import sqlite3
import uuid
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path


@dataclass(frozen=True)
class Document:
    id: str
    title: str
    original_file_name: str
    import_date_utc: str
    status: str


def utc_now_iso() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat()


def normalize_text(value: str) -> str:
    return " ".join(value.lower().split())


def read_schema(schema_path: Path) -> str:
    return schema_path.read_text(encoding="utf-8")


def init_db(db_path: Path, schema_path: Path) -> None:
    db_path.parent.mkdir(parents=True, exist_ok=True)
    schema = read_schema(schema_path)
    with sqlite3.connect(db_path) as conn:
        conn.executescript(schema)
        conn.commit()


def sha256_of_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as file_obj:
        for chunk in iter(lambda: file_obj.read(64 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def import_document(db_path: Path, file_path: Path, title: str | None = None) -> str:
    if not file_path.exists() or not file_path.is_file():
        raise FileNotFoundError(f"File not found: {file_path}")

    content = file_path.read_text(encoding="utf-8", errors="ignore")
    document_id = str(uuid.uuid4())
    now = utc_now_iso()
    doc_title = title or file_path.stem
    mime_type = mimetypes.guess_type(file_path.name)[0] or "application/octet-stream"

    with sqlite3.connect(db_path) as conn:
        conn.execute(
            """
            INSERT INTO documents(
                id, title, original_file_name, mime_type, sha256, page_count,
                import_date_utc, status, is_deleted
            ) VALUES (?, ?, ?, ?, ?, 1, ?, 'Imported', 0)
            """,
            (
                document_id,
                doc_title,
                file_path.name,
                mime_type,
                sha256_of_file(file_path),
                now,
            ),
        )
        conn.execute(
            """
            INSERT INTO document_files(document_id, original_path, file_size_bytes)
            VALUES (?, ?, ?)
            """,
            (document_id, str(file_path.resolve()), file_path.stat().st_size),
        )
        conn.execute(
            """
            INSERT INTO document_text(document_id, plain_text, normalized_text, last_ocr_utc)
            VALUES (?, ?, ?, ?)
            """,
            (document_id, content, normalize_text(content), now),
        )
        conn.execute(
            """
            INSERT INTO fts_document_text(document_id, title, plain_text)
            VALUES (?, ?, ?)
            """,
            (document_id, doc_title, content),
        )
        conn.commit()

    return document_id


def list_documents(db_path: Path) -> list[Document]:
    with sqlite3.connect(db_path) as conn:
        rows = conn.execute(
            """
            SELECT id, title, original_file_name, import_date_utc, status
            FROM documents
            WHERE is_deleted = 0
            ORDER BY import_date_utc DESC
            """
        ).fetchall()

    return [Document(*row) for row in rows]


def search_documents(db_path: Path, query: str) -> list[Document]:
    with sqlite3.connect(db_path) as conn:
        rows = conn.execute(
            """
            SELECT d.id, d.title, d.original_file_name, d.import_date_utc, d.status
            FROM fts_document_text f
            JOIN documents d ON d.id = f.document_id
            WHERE fts_document_text MATCH ? AND d.is_deleted = 0
            ORDER BY bm25(fts_document_text), d.import_date_utc DESC
            """,
            (query,),
        ).fetchall()

    return [Document(*row) for row in rows]


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="DocuDesk Lite (SQLite + FTS)")
    parser.add_argument("--db", default="data/docudesk_lite.db", help="Path to SQLite database")
    parser.add_argument("--schema", default="ddl_v1.sql", help="Path to schema SQL file")

    subparsers = parser.add_subparsers(dest="command", required=True)

    subparsers.add_parser("init-db", help="Initialize database")

    import_parser = subparsers.add_parser("import", help="Import a text/PDF/image file")
    import_parser.add_argument("file", help="File path")
    import_parser.add_argument("--title", help="Optional title")

    subparsers.add_parser("list", help="List imported documents")

    search_parser = subparsers.add_parser("search", help="Search in indexed text")
    search_parser.add_argument("query", help="FTS query")

    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    db_path = Path(args.db)
    schema_path = Path(args.schema)

    if args.command == "init-db":
        init_db(db_path, schema_path)
        print(f"Initialized database at {db_path}")
        return 0

    if not db_path.exists():
        parser.error("Database does not exist. Run 'init-db' first.")

    if args.command == "import":
        document_id = import_document(db_path, Path(args.file), args.title)
        print(f"Imported document {document_id}")
        return 0

    if args.command == "list":
        docs = list_documents(db_path)
        for doc in docs:
            print(f"{doc.id}\t{doc.title}\t{doc.original_file_name}\t{doc.import_date_utc}\t{doc.status}")
        return 0

    if args.command == "search":
        docs = search_documents(db_path, args.query)
        for doc in docs:
            print(f"{doc.id}\t{doc.title}\t{doc.original_file_name}\t{doc.import_date_utc}\t{doc.status}")
        return 0

    parser.error(f"Unknown command: {args.command}")
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
