const fs = require('fs');

let css = fs.readFileSync('app.css', 'utf-8');

// Add global styles for the visual improvements
css += `
/* Implicit Glitch and Angled borders */
.nc-nav-link:hover {
    animation: glitch-anim 0.25s infinite;
    color: var(--nc-red) !important;
    text-shadow: 2px 0 var(--nc-cyan), -2px 0 var(--nc-gold);
}

.news-article, .market-item, .hwz-card, .setup-box, .settings-box {
    clip-path: polygon(15px 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%, 0 15px);
}
`;

fs.writeFileSync('app.css', css);
console.log('Appended global glitch and angled classes.');
