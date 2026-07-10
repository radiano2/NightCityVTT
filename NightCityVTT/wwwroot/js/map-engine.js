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
let highlights = [];            // [{col,row}] movement range cells (blue)
let attackHighlights = [];      // [{col,row}] weapon attack range cells (red)

let particles = [];
let animating = false;

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
    ctx = canvas.getContext('2d', { alpha: false }); // Better performance if opaque

    canvas.onmousedown = onMouseDown;
    canvas.onmousemove = onMouseMove;
    canvas.onmouseup   = onMouseUp;
    canvas.onmouseleave= onMouseLeave;
    canvas.oncontextmenu = (e) => { e.preventDefault(); onRightClick(e); };
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

export function setAttackRange(cells) {
    attackHighlights = cells || [];
    render();
}

export function clearAttackRange() {
    attackHighlights = [];
    render();
}

export function selectToken(tokenId) {
    selectedTokenId = tokenId;
    render();
}

// ── Render ────────────────────────────────────────────────────────────────

function render() {
    if (!ctx) return;
    ctx.globalCompositeOperation = 'source-over';
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

    // Movement highlights (blue)
    for (const h of highlights) {
        const x = h.col * tileSize, y = h.row * tileSize;
        ctx.fillStyle   = 'rgba(0,210,255,0.12)';
        ctx.fillRect(x + 1, y + 1, tileSize - 2, tileSize - 2);
        ctx.strokeStyle = 'rgba(0,210,255,0.55)';
        ctx.lineWidth   = 1;
        ctx.strokeRect(x + 1, y + 1, tileSize - 2, tileSize - 2);
    }

    // Attack range highlights (red)
    for (const h of attackHighlights) {
        const x = h.col * tileSize, y = h.row * tileSize;
        ctx.fillStyle   = 'rgba(255,40,40,0.09)';
        ctx.fillRect(x + 1, y + 1, tileSize - 2, tileSize - 2);
        ctx.strokeStyle = 'rgba(255,40,40,0.35)';
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

    // Line of Sight
    if (selectedTokenId && editorMode === null && !isPainting) {
        const token = tokens.find(t => t.id === selectedTokenId);
        if (token) {
            drawLineOfSight(token);
        }
    }

    if (!animating && particles.length > 0) {
        animating = true;
        requestAnimationFrame(animateParticles);
    }
}

function drawLineOfSight(token) {
    const cx = token.col * tileSize + tileSize / 2;
    const cy = token.row * tileSize + tileSize / 2;
    const radius = tileSize * 12;
    
    ctx.fillStyle = 'rgba(0,0,0,0.85)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    ctx.globalCompositeOperation = 'destination-out';
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    
    for (let angle = 0; angle < Math.PI * 2; angle += 0.05) {
        let dx = Math.cos(angle);
        let dy = Math.sin(angle);
        let dist = 0;
        let hitX = cx;
        let hitY = cy;
        
        while (dist < radius) {
            hitX += dx * 2;
            hitY += dy * 2;
            dist += 2;
            const c = Math.floor(hitX / tileSize);
            const r = Math.floor(hitY / tileSize);
            if (c < 0 || c >= mapWidth || r < 0 || r >= mapHeight) break;
            const t = tiles[`${c},${r}`] || 0;
            if (t === 2 || t === 3 || t === 4) break; // block vision
        }
        ctx.lineTo(hitX, hitY);
    }
    ctx.closePath();
    
    const grd = ctx.createRadialGradient(cx, cy, 0, cx, cy, radius);
    grd.addColorStop(0, 'rgba(0,0,0,1)');
    grd.addColorStop(0.8, 'rgba(0,0,0,0.8)');
    grd.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = grd;
    ctx.fill();
    
    ctx.globalCompositeOperation = 'source-over';
}

export function triggerAttackEffect(attackerId, targetId, damageStr, isHit) {
    const a = tokens.find(t => t.id === attackerId);
    const t = tokens.find(t => t.id === targetId);
    if (!a || !t) return;
    
    const ax = a.col * tileSize + tileSize/2;
    const ay = a.row * tileSize + tileSize/2;
    const tx = t.col * tileSize + tileSize/2;
    const ty = t.row * tileSize + tileSize/2;
    
    particles.push({ type: 'muzzle', x: ax, y: ay, tx, ty, life: 1.0 });
    particles.push({ type: 'tracer', x: ax, y: ay, tx, ty, life: 1.0, isHit });
    
    if (isHit) {
        particles.push({ type: 'floater', x: tx, y: ty, text: damageStr, color: '#ff2040', life: 1.0 });
    } else {
        particles.push({ type: 'floater', x: tx, y: ty, text: 'MISS', color: '#aaaaaa', life: 1.0 });
    }
    
    if (!animating) { animating = true; requestAnimationFrame(animateParticles); }
}

function animateParticles() {
    render(); 
    
    let active = false;
    for (let i = particles.length - 1; i >= 0; i--) {
        const p = particles[i];
        p.life -= 0.03;
        if (p.life <= 0) {
            particles.splice(i, 1);
            continue;
        }
        active = true;
        
        if (p.type === 'muzzle') {
            const size = tileSize * p.life;
            ctx.fillStyle = `rgba(255, 200, 50, ${p.life})`;
            ctx.beginPath();
            ctx.arc(p.x, p.y, size, 0, Math.PI*2);
            ctx.fill();
        } else if (p.type === 'tracer') {
            const progress = 1.0 - p.life; 
            const curX = p.x + (p.tx - p.x) * progress;
            const curY = p.y + (p.ty - p.y) * progress;
            ctx.strokeStyle = p.isHit ? `rgba(255, 255, 50, ${p.life})` : `rgba(150, 150, 150, ${p.life})`;
            ctx.lineWidth = 3;
            ctx.beginPath();
            ctx.moveTo(p.x, p.y);
            ctx.lineTo(curX, curY);
            ctx.stroke();
        } else if (p.type === 'floater') {
            const rise = (1.0 - p.life) * 40;
            ctx.fillStyle = p.color;
            ctx.globalAlpha = p.life;
            ctx.font = 'bold 22px "Share Tech Mono"';
            ctx.textAlign = 'center';
            ctx.fillText(p.text, p.x, p.y - rise - 10);
            ctx.globalAlpha = 1.0;
        }
    }
    
    if (active) requestAnimationFrame(animateParticles);
    else animating = false;
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
    } else if (type === 4) {
        // Half-cover — orange tinted block with dashed outline
        ctx.fillStyle   = '#1a0d00';
        ctx.fillRect(x, y, tileSize, tileSize);
        ctx.strokeStyle = 'rgba(255,140,0,0.6)';
        ctx.lineWidth   = 1.5;
        ctx.setLineDash([4, 4]);
        ctx.strokeRect(x + 2, y + 2, tileSize - 4, tileSize - 4);
        ctx.setLineDash([]);
        ctx.fillStyle   = 'rgba(255,140,0,0.7)';
        ctx.font        = `${Math.floor(tileSize * 0.35)}px monospace`;
        ctx.textAlign   = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText('◒', x + tileSize / 2, y + tileSize / 2);
    } else if (type === 5) {
        // Hazard — yellow striped hazard pattern
        ctx.fillStyle   = '#1a1a00';
        ctx.fillRect(x, y, tileSize, tileSize);
        ctx.strokeStyle = 'rgba(255,255,0,0.8)';
        ctx.lineWidth   = 1;
        ctx.strokeRect(x, y, tileSize, tileSize);
        ctx.strokeStyle = 'rgba(255,255,0,0.2)';
        ctx.lineWidth   = 2;
        ctx.beginPath();
        for (let d = -tileSize; d < tileSize * 2; d += 8) {
            ctx.moveTo(x + d, y);
            ctx.lineTo(x + d + tileSize, y + tileSize);
        }
        ctx.stroke();
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

    const hit = tokens.find(t => t.col === col && t.row === row);
    if (hit) {
        if (selectedTokenId && hit.id !== selectedTokenId) {
            // Context menu!
            if (dotnetRef) {
                // Determine offset relative to canvas
                const rect = canvas.getBoundingClientRect();
                const offsetX = e.clientX - rect.left;
                const offsetY = e.clientY - rect.top;
                dotnetRef.invokeMethodAsync('OnTokenTargeted', hit.id, offsetX, offsetY);
            }
            return;
        }
        dragTokenId  = hit.id;
        dragCursorX = e.clientX;
        dragCursorY = e.clientY;
        selectedTokenId = hit.id;
        if (dotnetRef) dotnetRef.invokeMethodAsync('OnTokenSelected', hit.id);
        render();
        return;
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
    const typeMap = { floor: 1, wall: 2, door: 3, halfcover: 4, hazard: 5 };
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
