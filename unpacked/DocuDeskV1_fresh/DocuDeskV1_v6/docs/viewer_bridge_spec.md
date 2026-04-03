# Viewer-Bridge-Spezifikation

## Aktueller Stand
Der Viewer lädt lokale Assets über WebView2 und `SetVirtualHostNameToFolderMapping`.
Die WPF-Seite sendet aktuell Nachrichten vom Typ `openDocument` an die Web-Komponente.

## Aktuelle Nachricht
```json
{
  "type": "openDocument",
  "path": "C:\Users\...\document.pdf"
}
```

## Geplante Erweiterungen
- `openSearchResult` mit `documentId`, `page`, `query`, `bounds`
- `toggleOcrLayer`
- `toggleAnnotations`
- `copySelection`
- `exportSelection`
- `createAnnotation`

## Nächste Viewer-Stufe
Die nächste Ausbaustufe ersetzt die direkte PDF-Anzeige durch einen PDF.js-basierten Viewer, um Treffer-Highlights und Bounding-Boxes präzise auf Seitenebene zu projizieren.
