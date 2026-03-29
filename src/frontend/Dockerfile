# Stage 1: Build Angular application
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci

# Copy source code
COPY . .

# Build application
RUN npm run build

# Stage 2: Serve with nginx
FROM nginx:alpine

# Copy built files from build stage
COPY --from=build /app/dist/passkey-auth-frontend/browser /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Create certs directory and generate certificates if they don't exist
RUN mkdir -p /etc/nginx/certs
RUN if [ ! -f /etc/nginx/certs/cert.pem ]; then \
    apk add --no-cache openssl && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /etc/nginx/certs/key.pem \
    -out /etc/nginx/certs/cert.pem \
    -subj "/CN=localhost"; \
    fi

# Expose ports
EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]
