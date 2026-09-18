# syntax=docker/dockerfile:1.7

FROM node:24-alpine AS base
ENV NEXT_TELEMETRY_DISABLED=1
WORKDIR /app

FROM base AS deps
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN corepack enable && corepack prepare pnpm@11.24.0 --activate && pnpm install --frozen-lockfile

FROM deps AS builder
ARG NEXT_PUBLIC_API_BASE_URL=http://localhost:5059
ARG NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT=
ARG BUILD_VERSION=dev
ENV NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL \
    NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT=$NEXT_PUBLIC_CLIENT_ERROR_ENDPOINT \
    BUILD_VERSION=$BUILD_VERSION
COPY . .
RUN pnpm build

FROM base AS runner
ARG BUILD_VERSION=unknown
ENV NODE_ENV=production \
    PORT=3000 \
    HOSTNAME=0.0.0.0 \
    BUILD_VERSION=$BUILD_VERSION
WORKDIR /app

COPY --from=builder --chown=node:node /app/public ./public
COPY --from=builder --chown=node:node /app/.next/standalone ./
COPY --from=builder --chown=node:node /app/.next/static ./.next/static

USER node

EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://127.0.0.1:3000/api/health || exit 1

CMD ["node", "server.js"]
