# FinSight Branch Strategy

## Branches

### main

Production branch.

Changes reach `main` only through an approved pull request from `staging`.

### staging

Pre-production integration branch.

Changes reach `staging` through a pull request from `dev`.

### dev

Primary development integration branch.

Feature and fix branches target `dev`.

### feature/*

Short-lived branches used for new functionality.

Example:

feature/transaction-categorization

### fix/*

Short-lived branches used for bug fixes.

Example:

fix/duplicate-transaction-detection

## Flow

feature/*
    ↓
   dev
    ↓
 staging
    ↓
   main

## Rules

1. No direct pushes to `main`.
2. No direct pushes to `staging`.
3. Prefer pull requests for `dev`.
4. CI must pass before merging.
5. Security checks must pass before merging.
6. Production changes must originate from `staging`.
