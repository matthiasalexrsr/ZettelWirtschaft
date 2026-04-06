import tempfile
import unittest
from pathlib import Path

from app.docudesk_lite import import_document, init_db, list_documents, search_documents


class DocuDeskLiteTests(unittest.TestCase):
    def test_import_list_search_flow(self) -> None:
        with tempfile.TemporaryDirectory() as tmp_dir:
            root = Path(tmp_dir)
            db_path = root / "docudesk.db"
            source_file = root / "invoice.txt"
            source_file.write_text("Invoice 2026 from Power Utility", encoding="utf-8")

            init_db(db_path, Path("ddl_v1.sql"))
            document_id = import_document(db_path, source_file, title="Power bill")

            self.assertTrue(document_id)

            docs = list_documents(db_path)
            self.assertEqual(1, len(docs))
            self.assertEqual("Power bill", docs[0].title)

            hits = search_documents(db_path, "invoice")
            self.assertEqual(1, len(hits))
            self.assertEqual(document_id, hits[0].id)


if __name__ == "__main__":
    unittest.main()
