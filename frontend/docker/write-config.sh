#!/bin/sh
cat > /usr/share/nginx/html/config.js <<EOF
window.APP_CONFIG = { apiUrl: "${API_URL:-http://localhost:8080}" };
EOF
