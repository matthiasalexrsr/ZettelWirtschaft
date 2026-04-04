function byId(id) { return document.getElementById(id); }

let currentState = { path: '', overlay: null, highlightQuery: '' };
let selectedBlocks = [];
let annotationMode = null;

function setHeader(title, meta, status) {
  byId('title').textContent = title || 'Dokument';
  byId('meta').textContent = meta || '';
  byId('status').textContent = status || '';
}

function renderEmpty(message) {
  setHeader('Kein Dokument ausgewählt', '', '');
  byId('content').innerHTML = `<div class="empty">${message}</div>`;
  selectedBlocks = [];
}

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

function toFileUrl(path) {
  const normalized = String(path).replaceAll('\\', '/');
  return 'file:///' + normalized.replace(/^([A-Za-z]):/, '$1:');
}

function getExtension(path) {
  const parts = String(path).split('.');
  return parts.length > 1 ? parts.pop().toLowerCase() : '';
}

function getFileName(path) {
  return String(path).split(/[\\/]/).pop();
}

function parseHighlightTerms(query) {
  return String(query || '')
    .toLowerCase()
    .split(/[^\p{L}\p{N}]+/u)
    .map(x => x.trim())
    .filter(x => x.length >= 2);
}

function blockMatchesQuery(block, terms) {
  if (!terms.length) return false;
  const text = String(block?.text || '').toLowerCase();
  return terms.some(term => text.includes(term));
}

function resetSelection() {
  selectedBlocks = [];
  document.querySelectorAll('.overlay-block.selected').forEach(el => el.classList.remove('selected'));
  updateStatus();
}

function toggleBlockSelection(block, element) {
  const key = `${block.pageNumber}:${block.index}`;
  const idx = selectedBlocks.findIndex(x => x.key === key);
  if (idx >= 0) {
    selectedBlocks.splice(idx, 1);
    element.classList.remove('selected');
  } else {
    selectedBlocks.push({ key, text: block.text || '', pageNumber: block.pageNumber });
    element.classList.add('selected');
  }
  updateStatus();
}

function setAnnotationMode(mode) {
  annotationMode = annotationMode === mode ? null : mode;
  byId('annotation-rect-btn').classList.toggle('is-active', annotationMode === 'rectangle');
  document.body.classList.toggle('annotation-mode', !!annotationMode);
  updateStatus(annotationMode ? 'Rechteckmodus aktiv' : null);
}

function updateStatus(extra) {
  if (extra) {
    byId('status').textContent = extra;
    return;
  }
  if (selectedBlocks.length > 0) {
    byId('status').textContent = `${selectedBlocks.length} Block${selectedBlocks.length === 1 ? '' : 'e'} ausgewählt`;
    return;
  }
  const terms = parseHighlightTerms(currentState.highlightQuery);
  if (terms.length > 0) {
    byId('status').textContent = `Treffer für: ${terms.join(', ')}`;
    return;
  }
  byId('status').textContent = annotationMode ? 'Rechteckmodus aktiv' : '';
}

function copyTextToHost(text) {
  const payload = { type: 'copyText', text };
  if (window.chrome?.webview) {
    window.chrome.webview.postMessage(payload);
  } else if (navigator.clipboard?.writeText) {
    navigator.clipboard.writeText(text);
  }
  updateStatus('Text kopiert');
  window.setTimeout(() => updateStatus(), 1500);
}

function getAllOverlayText() {
  const pages = currentState.overlay?.pages || [];
  return pages
    .map(page => page.pageText || (page.blocks || []).map(block => block.text || '').join(' '))
    .filter(Boolean)
    .join('\n\n');
}

function getPageText(pageNumber) {
  const page = (currentState.overlay?.pages || []).find(x => x.pageNumber === pageNumber);
  if (!page) return '';
  return page.pageText || (page.blocks || []).map(block => block.text || '').join(' ');
}

function renderOverlayBlocks(container, blocks, sourceWidth, sourceHeight, renderedWidth, renderedHeight, pageNumber) {
  container.innerHTML = '';
  const sx = renderedWidth / Math.max(sourceWidth || 1, 1);
  const sy = renderedHeight / Math.max(sourceHeight || 1, 1);
  const terms = parseHighlightTerms(currentState.highlightQuery);

  for (let index = 0; index < (blocks || []).length; index++) {
    const block = blocks[index];
    const el = document.createElement('div');
    el.className = 'overlay-block';
    el.title = block.text || '';
    el.style.left = `${(block.x || 0) * sx}px`;
    el.style.top = `${(block.y || 0) * sy}px`;
    el.style.width = `${Math.max((block.width || 0) * sx, 2)}px`;
    el.style.height = `${Math.max((block.height || 0) * sy, 2)}px`;

    const enrichedBlock = { ...block, pageNumber, index };
    if (blockMatchesQuery(block, terms)) {
      el.classList.add('search-match');
    }

    el.addEventListener('click', event => {
      if (annotationMode) return;
      event.preventDefault();
      event.stopPropagation();
      toggleBlockSelection(enrichedBlock, el);
    });

    container.appendChild(el);
  }
}

function renderAnnotations(container, annotations, sourceWidth, sourceHeight, renderedWidth, renderedHeight) {
  container.innerHTML = '';
  const sx = renderedWidth / Math.max(sourceWidth || 1, 1);
  const sy = renderedHeight / Math.max(sourceHeight || 1, 1);

  for (const item of (annotations || [])) {
    const el = document.createElement('div');
    el.className = 'annotation-rect';
    el.style.left = `${(item.x || 0) * sx}px`;
    el.style.top = `${(item.y || 0) * sy}px`;
    el.style.width = `${Math.max((item.width || 0) * sx, 2)}px`;
    el.style.height = `${Math.max((item.height || 0) * sy, 2)}px`;
    el.style.borderColor = item.color || 'rgba(239, 68, 68, 0.95)';
    el.style.background = 'rgba(239, 68, 68, 0.08)';
    el.dataset.label = item.text || 'Anmerkung';
    container.appendChild(el);
  }
}

function attachAnnotationDrawing(stage, annotationLayer, sourceWidth, sourceHeight, pageNumber) {
  let start = null;
  let draft = null;

  const cleanupDraft = () => {
    if (draft) {
      draft.remove();
      draft = null;
    }
  };

  stage.onmousedown = event => {
    if (annotationMode !== 'rectangle' || event.button !== 0) return;
    event.preventDefault();
    const rect = stage.getBoundingClientRect();
    start = { x: event.clientX - rect.left, y: event.clientY - rect.top };
    cleanupDraft();
    draft = document.createElement('div');
    draft.className = 'annotation-draft';
    annotationLayer.appendChild(draft);
  };

  stage.onmousemove = event => {
    if (!start || !draft || annotationMode !== 'rectangle') return;
    const rect = stage.getBoundingClientRect();
    const current = { x: event.clientX - rect.left, y: event.clientY - rect.top };
    const left = Math.min(start.x, current.x);
    const top = Math.min(start.y, current.y);
    const width = Math.abs(start.x - current.x);
    const height = Math.abs(start.y - current.y);
    draft.style.left = `${left}px`;
    draft.style.top = `${top}px`;
    draft.style.width = `${width}px`;
    draft.style.height = `${height}px`;
  };

  stage.onmouseup = event => {
    if (!start || annotationMode !== 'rectangle') return;
    const rect = stage.getBoundingClientRect();
    const end = { x: event.clientX - rect.left, y: event.clientY - rect.top };
    const left = Math.min(start.x, end.x);
    const top = Math.min(start.y, end.y);
    const width = Math.abs(start.x - end.x);
    const height = Math.abs(start.y - end.y);
    cleanupDraft();
    start = null;

    if (width < 6 || height < 6) {
      updateStatus('Annotation zu klein');
      window.setTimeout(() => updateStatus(), 1200);
      return;
    }

    const scaleX = sourceWidth / Math.max(rect.width, 1);
    const scaleY = sourceHeight / Math.max(rect.height, 1);
    const text = window.prompt('Optionaler Hinweis zur Markierung:', '') || '';
    const annotation = {
      pageNumber,
      annotationType: 'rectangle',
      x: Math.round(left * scaleX * 100) / 100,
      y: Math.round(top * scaleY * 100) / 100,
      width: Math.round(width * scaleX * 100) / 100,
      height: Math.round(height * scaleY * 100) / 100,
      text,
      color: '#ef4444'
    };

    if (window.chrome?.webview) {
      window.chrome.webview.postMessage({ type: 'annotationCreated', annotation });
    }
    updateStatus('Annotation gespeichert');
    window.setTimeout(() => updateStatus(), 1200);
  };

  stage.onmouseleave = () => {
    if (start && annotationMode === 'rectangle') {
      cleanupDraft();
      start = null;
    }
  };
}

function wireGlobalButtons() {
  byId('copy-selection-btn').onclick = () => {
    if (!selectedBlocks.length) {
      updateStatus('Keine Auswahl');
      window.setTimeout(() => updateStatus(), 1200);
      return;
    }
    copyTextToHost(selectedBlocks.map(x => x.text).join(' '));
  };

  byId('copy-all-btn').onclick = () => {
    const text = getAllOverlayText();
    if (!text) {
      updateStatus('Kein OCR-Text vorhanden');
      window.setTimeout(() => updateStatus(), 1200);
      return;
    }
    copyTextToHost(text);
  };

  byId('annotation-rect-btn').onclick = () => setAnnotationMode('rectangle');
}

function renderImage(docPath, overlay) {
  const fileUrl = toFileUrl(docPath);
  const title = getFileName(docPath);
  setHeader(title, `Bild · ${docPath}`, overlay?.pages?.length ? 'OCR-Overlay aktiv' : '');
  byId('content').innerHTML = `
    <div class="page-shell">
      <div class="page-label">
        <span>Seite 1</span>
        <button class="page-copy-btn" type="button" data-page-copy="1">Seite kopieren</button>
      </div>
      <div class="image-stage annotation-surface" id="image-stage">
        <img id="document-image" src="${fileUrl}" alt="${escapeHtml(title)}" />
        <div id="image-overlay" class="overlay-root"></div>
        <div id="image-annotations" class="annotation-layer"></div>
      </div>
    </div>`;

  document.querySelector('[data-page-copy="1"]')?.addEventListener('click', () => {
    const text = getPageText(1);
    if (text) copyTextToHost(text);
  });

  const image = byId('document-image');
  image.addEventListener('load', () => {
    resetSelection();
    const stage = byId('image-stage');
    const ocrLayer = byId('image-overlay');
    const annotationLayer = byId('image-annotations');
    const firstPage = overlay?.pages?.[0] || { pageNumber: 1, widthPx: image.naturalWidth || image.width, heightPx: image.naturalHeight || image.height, blocks: [], annotations: [] };
    renderOverlayBlocks(ocrLayer, firstPage.blocks || [], firstPage.widthPx || image.naturalWidth || image.width, firstPage.heightPx || image.naturalHeight || image.height, image.clientWidth, image.clientHeight, 1);
    renderAnnotations(annotationLayer, firstPage.annotations || [], firstPage.widthPx || image.naturalWidth || image.width, firstPage.heightPx || image.naturalHeight || image.height, image.clientWidth, image.clientHeight);
    attachAnnotationDrawing(stage, annotationLayer, firstPage.widthPx || image.naturalWidth || image.width, firstPage.heightPx || image.naturalHeight || image.height, 1);
    updateStatus();
  });
}

async function tryLoadPdfJs() {
  if (window.pdfjsLib) {
    return true;
  }

  const workerPath = 'https://app.docudesk.viewer/pdfjs/pdf.worker.min.js';
  const scriptPath = 'https://app.docudesk.viewer/pdfjs/pdf.min.js';

  return new Promise((resolve) => {
    const existing = document.querySelector('script[data-pdfjs="1"]');
    if (existing) {
      existing.addEventListener('load', () => resolve(!!window.pdfjsLib));
      existing.addEventListener('error', () => resolve(false));
      return;
    }

    const script = document.createElement('script');
    script.src = scriptPath;
    script.dataset.pdfjs = '1';
    script.onload = () => {
      if (window.pdfjsLib) {
        window.pdfjsLib.GlobalWorkerOptions.workerSrc = workerPath;
        resolve(true);
      } else {
        resolve(false);
      }
    };
    script.onerror = () => resolve(false);
    document.head.appendChild(script);
  });
}

async function renderPdfWithPdfJs(docPath, overlay) {
  const ok = await tryLoadPdfJs();
  if (!ok) {
    return false;
  }

  const fileUrl = toFileUrl(docPath);
  const pdf = await window.pdfjsLib.getDocument(fileUrl).promise;
  const pages = overlay?.pages || [];
  const overlayMap = new Map(pages.map(p => [p.pageNumber, p]));
  const title = getFileName(docPath);
  setHeader(title, `PDF · ${docPath}`, 'PDF.js aktiv');
  byId('content').innerHTML = '';
  resetSelection();

  for (let pageNumber = 1; pageNumber <= pdf.numPages; pageNumber++) {
    const page = await pdf.getPage(pageNumber);
    const viewport = page.getViewport({ scale: 1.35 });
    const pageShell = document.createElement('div');
    pageShell.className = 'page-shell';
    pageShell.innerHTML = `
      <div class="page-label">
        <span>Seite ${pageNumber}</span>
        <button class="page-copy-btn" type="button" data-page-copy="${pageNumber}">Seite kopieren</button>
      </div>
      <div class="page-stage annotation-surface">
        <canvas></canvas>
        <div class="page-overlay overlay-root"></div>
        <div class="annotation-layer"></div>
      </div>`;

    const canvas = pageShell.querySelector('canvas');
    const stage = pageShell.querySelector('.page-stage');
    const overlayEl = pageShell.querySelector('.page-overlay');
    const annotationLayer = pageShell.querySelector('.annotation-layer');
    const context = canvas.getContext('2d');
    canvas.width = viewport.width;
    canvas.height = viewport.height;
    stage.style.width = `${viewport.width}px`;
    stage.style.height = `${viewport.height}px`;

    await page.render({ canvasContext: context, viewport }).promise;

    const pageOverlay = overlayMap.get(pageNumber) || { pageNumber, widthPx: viewport.width, heightPx: viewport.height, blocks: [], annotations: [] };
    renderOverlayBlocks(overlayEl, pageOverlay.blocks || [], pageOverlay.widthPx || viewport.width, pageOverlay.heightPx || viewport.height, viewport.width, viewport.height, pageNumber);
    renderAnnotations(annotationLayer, pageOverlay.annotations || [], pageOverlay.widthPx || viewport.width, pageOverlay.heightPx || viewport.height, viewport.width, viewport.height);
    attachAnnotationDrawing(stage, annotationLayer, pageOverlay.widthPx || viewport.width, pageOverlay.heightPx || viewport.height, pageNumber);

    pageShell.querySelector(`[data-page-copy="${pageNumber}"]`)?.addEventListener('click', () => {
      const text = getPageText(pageNumber);
      if (text) copyTextToHost(text);
    });

    byId('content').appendChild(pageShell);
  }

  updateStatus();
  return true;
}

function renderPdfFallback(docPath, overlay) {
  const fileUrl = toFileUrl(docPath);
  const title = getFileName(docPath);
  const status = overlay?.pages?.length ? 'OCR erfasst · PDF.js-Dateien fehlen' : 'PDF-Fallback';
  setHeader(title, `PDF · ${docPath}`, status);
  byId('content').innerHTML = `
    <div class="info-box">
      <div>
        <div>PDF.js ist noch nicht lokal hinterlegt. Daher wird der Browser-PDF-Viewer als Fallback verwendet.</div>
        <div class="code">Legen Sie pdf.min.js und pdf.worker.min.js unter viewer-assets/pdfjs/ ab, um Canvas-Rendering und OCR-Overlays direkt auf PDF-Seiten zu aktivieren.</div>
      </div>
    </div>
    <iframe class="pdf-fallback" src="${fileUrl}"></iframe>`;
  resetSelection();
  updateStatus();
}

async function loadDocument(docPath, overlay) {
  if (!docPath) {
    renderEmpty('Bitte links ein Dokument auswählen.');
    return;
  }

  const ext = getExtension(docPath);
  if (ext === 'pdf') {
    try {
      const rendered = await renderPdfWithPdfJs(docPath, overlay);
      if (!rendered) {
        renderPdfFallback(docPath, overlay);
      }
    } catch (error) {
      console.error(error);
      renderPdfFallback(docPath, overlay);
    }
    return;
  }

  if (['png', 'jpg', 'jpeg', 'bmp', 'gif', 'tif', 'tiff', 'webp'].includes(ext)) {
    renderImage(docPath, overlay);
    return;
  }

  setHeader(getFileName(docPath), `${ext.toUpperCase()} · ${docPath}`, 'Kein Spezialviewer');
  byId('content').innerHTML = `<div class="info-box"><div>Für diesen Dateityp ist in V2 ein spezialisierter Viewer vorgesehen.<div class="code">${escapeHtml(docPath)}</div></div></div>`;
  resetSelection();
  updateStatus();
}

window.chrome?.webview?.addEventListener('message', event => {
  if (event.data && event.data.type === 'openDocument') {
    currentState = {
      path: event.data.path || '',
      overlay: event.data.overlay || null,
      highlightQuery: event.data.highlightQuery || ''
    };
    loadDocument(currentState.path, currentState.overlay);
  }
});

wireGlobalButtons();
renderEmpty('Bitte links ein Dokument auswählen.');
