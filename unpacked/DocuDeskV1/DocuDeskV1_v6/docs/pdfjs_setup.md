# PDF.js-Setup für den lokalen Viewer

Der Viewer unterstützt zwei Modi:

1. **PDF.js-Modus**
   - Rendern von PDF-Seiten auf Canvas
   - OCR-Overlay direkt auf den Seiten
   - Grundlage für spätere Trefferhervorhebung und Textselektion

2. **Fallback-Modus**
   - eingebauter Browser-PDF-Viewer in WebView2
   - kein direktes Overlay auf PDF-Seiten

## Lokale Dateien ablegen

Lege die folgenden Dateien in `src/DocuDesk.Viewer/viewer-assets/pdfjs/` ab:

- `pdf.min.js`
- `pdf.worker.min.js`

Danach werden PDF-Seiten im lokalen WebView2-Viewer per PDF.js gerendert.
