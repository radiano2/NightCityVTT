// NightCityVTT — Tactical Battle Map Engine
// ES module — imported via IJSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/map-engine.js")

let canvas, ctx;
let mapWidth = 20, mapHeight = 20, tileSize = 40;
let tiles = {};        // "col,row" -> type int (0=empty,1=floor,2=wall,3=door)
let tokens = [];       // TokenState[]
let dotnetRef = null;

// Interaction state
let editorMode = null;          // null | "floor" | "wall" | "door"
let isPainting = false;
let selectedTokenId = null;
let dragTokenId = null;
let dragCursorX = 0, dragCursorY = 0;
let highlights = [];            // [{col,row}] movement range cells

// Palette
const FILL = { 0: '#06060f', 1: '#0d1117', 2: '#0d0208', 3: '#061106' };
const TOKEN_COLORS = [
    '#ff3a3a','#3a9fff','#3aff6e','#ffb73a','#ff3aff','#3affff','#ff8c3a','#b83aff'
];

export function getTokenColors() { return TOKEN_COLORS; }

export function init(canvasId, w, h, ts) {
    canvas = document.getElementById(canvasId);
    if (!canvas) return false;
    mapWidth = w; mapHeight = h; tileSize = ts;
    canvas.width  = w * ts;
    canvas.height = h * ts;
    ctx = canvas.getContext('2d');

    canvas.addEventListener('mousedown',    onMouseDown);
    canvas.addEventListener('mousemove',    onMouseMove);
    canvas.addEventListener('mouseup',      onMouseUp);
    canvas.addEventListener('mouseleave',   onMouseLeave);
    canvas.addEventListener('contextmenu',  e => { e.preventDefault(); onRightClick(e); });
    return true;
}

export function setDotnetRef(ref) { dotnetRef = ref; }

export function loadMap(mapData) {
    mapWidth  = mapData.width;
    mapHeight = mapData.height;
    tileSize  = mapData.tileSize;
    canvas.width  = mapWidth  * tileSize;
    canvas.height = mapHeight * tileSize;
    tiles = {};
    for (const c of (mapData.tiles || [])) tiles[`${c.col},${c.row}`] = c.type;
    tokens = (mapData.tokens || []).map(t => ({ ...t }));
    render();
}

export function updateToken(t) {
    const idx = tokens.findIndex(x => x.id === t.id);
    if (idx >= 0) Object.assign(tokens[idx], t);
    else tokens.push({ ...t });
    render();
}

export function removeToken(tokenId) {
    tokens = tokens.filter(t => t.id !== tokenId);
    if (selectedTokenId === tokenId) { selectedTokenId = null; highlights = []; }
    render();
}

export function setEditorMode(mode) {
    editorMode = mode || null;
    selectedTokenId = null;
    highlights = [];
    render();
}

export function setHighlights(cells) {
    highlights = cells || [];
    render();
}

export function selectToken(tokenId) {
    selectedTokenId = tokenId;
    render();
}

// ── Render ────────────────────────────────────────────────────────────────

function render() {
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Background
    ctx.fillStyle = '#06060f';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Tiles
    for (let r = 0; r < mapHeight; r++) {
        for (let c = 0; c < mapWidth; c++) {
            drawTile(c, r, tiles[`${c},${r}`] || 0);
        }
    }

    // Movement highlights
    for (const h of highlights) {
        const x = h.col * tileSize, y = h.row * tileSize;
        ctx.fillStyle   = 'rgba(0,210,255,0.12)';
        ctx.fillRect(x + 1, y + 1, tileSize - 2, tileSize - 2);
        ctx.strokeStyle = 'rgba(0,210,255,0.55)';
        ctx.lineWidth   = 1;
        ctx.strokeRect(x + 1, y + 1, tileSize - 2, tileSize - 2);
    }

    // Tokens (skip the one being dragged — drawn at cursor below)
    for (const t of tokens) {
        if (t.id !== dragTokenId) drawToken(t, t.id === selectedTokenId);
    }

    // Drag ghost
    if (dragTokenId) {
        const src = tokens.find(t => t.id === dragTokenId);
        if (src) {
            const rect = canvas.getBoundingClientRect();
            const sx = (dragCursorX - rect.left) * (canvas.width  / rect.width);
            const sy = (dragCursorY - rect.top)  * (canvas.height / rect.height);
            const snapCol = Math.max(0, Math.min(mapWidth  - 1, Math.floor(sx / tileSize)));
            const snapRow = Math.max(0, Math.min(mapHeight - 1, Math.floor(sy / tileSize)));
            ctx.globalAlpha = 0.6;
            drawToken({ ...src, col: snapCol, row: snapRow }, false);
            ctx.globalAlpha = 1;
        }
    }
}

function drawTile(col, row, type) {
    const x = col * tileSize, y = row * tileSize;

    ctx.fillStyle = FILL[type] ?? FILL[0];
    ctx.fillRect(x, y, tileSize, tileSize);

    if (type === 1) {
        // Floor — subtle grid
        ctx.strokeStyle = '#1a2040';
        ctx.lineWidth   = 0.5;
        ctx.strokeRect(x, y, tileSize, tileSize);
    } else if (type === 2) {
        // Wall — red-tinted solid block with outline
        ctx.fillStyle   = '#1a0008';
        ctx.fillRect(x, y, tileSize, tileSize);
        ctx.fillStyle   = 'rgba(255,0,60,0.08)';
        ctx.fillRect(x, y, tileSize, tileSize);
        ctx.strokeStyle = 'rgba(255,0,60,0.5)';
        ctx.lineWidth   = 1.5;
        ctx.strokeRect(x + 1, y + 1, tileSize - 2, tileSize - 2);
        // Crosshatch
        ctx.strokeStyle = 'rgba(255,0,60,0.12)';
        ctx.lineWidth   = 0.5;
        ctx.beginPath();
        ctx.moveTo(x, y); ctx.lineTo(x + tileSize, y + tileSize);
        ctx.moveTo(x + tileSize, y); ctx.lineTo(x, y + tileSize);
        ctx.stroke();
    } else if (type === 3) {
        // Door — green outline + symbol
        ctx.fillStyle   = '#001a06';
        ctx.fillRect(x, y, tileSize, tileSize);
        ctx.strokeStyle = 'rgba(0,255,65,0.7)';
        ctx.lineWidth   = 1.5;
        ctx.strokeRect(x + 3, y + 3, tileSize - 6, tileSize - 6);
        ctx.fillStyle   = 'rgba(0,255,65,0.7)';
        ctx.font        = `${Math.floor(tileSize * 0.38)}px monospace`;
        ctx.textAlign   = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('▭', x + tileSize / 2, y + tileSize / 2);
    }

    // Universal faint grid
    ctx.strokeStyle = '#ffffff06';
    ctx.lineWidth   = 0.5;
    ctx.strokeRect(x, y, tileSize, tileSize);
}

function drawToken(token, selected) {
    const cx = token.col * tileSize + tileSize / 2;
    const cy = token.row * tileSize + tileSize / 2;
    const r  = tileSize * 0.34;

    // Glow halo
    const grd = ctx.createRadialGradient(cx, cy, 0, cx, cy, r * 2);
    grd.addColorStop(0, token.color + '60');
    grd.addColorStop(1, 'transparent');
    ctx.fillStyle = grd;
    ctx.beginPath();
    ctx.arc(cx, cy, r * 2, 0, Math.PI * 2);
    ctx.fill();

    // Selection ring
    if (selected) {
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth   = 2;
        ctx.beginPath();
        ctx.arc(cx, cy, r + 4, 0, Math.PI * 2);
        ctx.stroke();
    }

    // Token circle
    ctx.fillStyle = token.color;
    ctx.beginPath();
    ctx.arc(cx, cy, r, 0, Math.PI * 2);
    ctx.fill();

    // Border
    ctx.strokeStyle = 'rgba(255,255,255,0.5)';
    ctx.lineWidth   = 1.5;
    ctx.stroke();

    // Initials
    ctx.fillStyle    = 'rgba(0,0,0,0.85)';
    ctx.font         = `bold ${Math.floor(tileSize * 0.28)}px "Share Tech Mono", monospace`;
    ctx.textAlign    = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText((token.handle || '??').substring(0, 3).toUpperCase(), cx, cy);

    // Handle label below
    ctx.fillStyle    = token.color;
    ctx.font         = `${Math.floor(tileSize * 0.2)}px "Share Tech Mono", monospace`;
    ctx.textAlign    = 'center';
    ctx.textBaseline = 'top';
    ctx.fillText(token.handle, cx, cy + r + 2);
}

// ── Interaction ──────────────────────────────────────────────────────────

function cellAt(e) {
    const rect = canvas.getBoundingClientRect();
    const sx   = (e.clientX - rect.left) * (canvas.width  / rect.width);
    const sy   = (e.clientY - rect.top)  * (canvas.height / rect.height);
    return {
        col: Math.max(0, Math.min(mapWidth  - 1, Math.floor(sx / tileSize))),
        row: Math.max(0, Math.min(mapHeight - 1, Math.floor(sy / tileSize)))
    };
}

function onMouseDown(e) {
    if (e.button !== 0) return;
    const { col, row } = cellAt(e);

    if (editorMode) {
        isPainting = true;
        paintAt(col, row);
        return;
    }

    // Play mode — check for token hit
    const hit = tokens.find(t => t.col === col && t.row === row);
    if (hit) {
        dragTokenId  = hit.id;
        dragCursorX  = e.clientX;
        dragCursorY  = e.clientY;
        // Select and highlight movement range
        selectedTokenId = hit.id;
        if (dotnetRef) dotnetRef.invokeMethodAsync('OnTokenSelected', hit.id);
    } else {
        selectedTokenId = null;
        highlights = [];
        render();
    }
}

function onMouseMove(e) {
    dragCursorX = e.clientX;
    dragCursorY = e.clientY;

    if (dragTokenId) {
        render(); // redraws ghost at new position
        return;
    }
    if (isPainting && editorMode) {
        const { col, row } = cellAt(e);
        paintAt(col, row);
    }
}

function onMouseUp(e) {
    if (dragTokenId) {
        const { col, row } = cellAt(e);
        const src = tokens.find(t => t.id === dragTokenId);
        if (src && (src.col !== col || src.row !== row)) {
            src.col = col;
            src.row = row;
            if (dotnetRef) dotnetRef.invokeMethodAsync('OnTokenMoved', dragTokenId, col, row);
        }
        dragTokenId = null;
        render();
        return;
    }
    if (isPainting) {
        isPainting = false;
        flushTiles();
    }
}

function onMouseLeave() {
    if (isPainting) { isPainting = false; flushTiles(); }
    if (dragTokenId) {
        dragTokenId = null;
        render();
    }
}

function onRightClick(e) {
    if (!editorMode) return;
    const { col, row } = cellAt(e);
    delete tiles[`${col},${row}`];
    render();
    flushTiles();
}

function paintAt(col, row) {
    const typeMap = { floor: 1, wall: 2, door: 3 };
    const t = typeMap[editorMode];
    if (t !== undefined) tiles[`${col},${row}`] = t;
    render();
}

function flushTiles() {
    if (!dotnetRef) return;
    const list = Object.entries(tiles).map(([key, type]) => {
        const [col, row] = key.split(',').map(Number);
        return { col, row, type };
    });
    dotnetRef.invokeMethodAsync('OnTilesChanged', list);
}
