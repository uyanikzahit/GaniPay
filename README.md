GaniPay Digital Wallet

GaniPay, the digital wallet project I conceptualized and architected, was not started as a feature-first coding exercise. It began as a structured attempt to understand and model the big picture of how a modern fintech wallet system should be designed.

The primary objective of GaniPay is not merely to build a functioning e-wallet, but to gain architectural clarity over an entire financial system — from identity and customer management to ledger-based accounting, payment orchestration, gateway routing, infrastructure design, and cloud-native deployment strategy.

Before writing APIs, I approached this project as an architectural blueprint. Domains were analyzed, bounded contexts were defined, workflow boundaries were designed, orchestration strategies were evaluated, infrastructure layers were planned, and deployment paths were considered. Every technical decision was taken with the intention of understanding how real-world fintech systems are structured beyond surface-level features.

GaniPay represents an architecture-first mindset. It is an effort to move beyond endpoints and isolated services, and instead understand how identity, accounting, payments, workflows, API gateway management, containerization, and scalable infrastructure operate together as a cohesive system.

The core ambition of GaniPay is not simply to build a wallet — but to master the system-level perspective behind it.



Table of Contents

Vision & Planning

High-Level Architecture

Repository Layout

DDD Bounded Contexts

Workflow Orchestration (Camunda 8 / Zeebe)

Worker Layer

API Gateway (APISIX + etcd)

Data Layer (PostgreSQL + DBeaver)

Redis (Caching & Ephemeral State Strategy)

Local Orchestration Options (Aspire vs Docker Compose)

Getting Started

Run with Aspire

Run Full Stack with Docker Compose

Run Mobile App (Expo)

Ports

Testing & Tooling

Observability & Operations

Security Notes

Kubernetes Strategy (k3d / AKS-ready)

Roadmap

Vision & Planning

This repository aims to model real fintech architecture decisions, not just CRUD:

Split domains via bounded contexts (DDD)

Make business flows explicit via BPMN (Camunda), not buried inside controllers

Execute steps via Zeebe workers with retries / timeouts / correlation

Run locally in a production-like stack (Docker, gateway, DB isolation, orchestration UI)

Connect to a real mobile client (React Native / Expo)

Prepare a cloud-native path to Kubernetes (k3d/AKS) with gateway + workers + services

High-Level Architecture

Request path (simplified):

Mobile Client calls the platform via APISIX (single entrypoint)

APISIX routes to:

Workflow API (orchestration boundary)

or directly to domain APIs when appropriate

Workflow API starts a Zeebe process instance (BPMN)

Zeebe dispatches jobs to workers

Workers call domain APIs (Identity / Customer / Accounting / Payments…)

Results are published back (callback/result pattern), and the client receives the outcome using correlation/response handling

Core building blocks in this repo:

GaniPay.Workflow.API → orchestration boundary, Zeebe integration, client-facing workflow endpoints

bpmn/ → BPMN process models (register/login/topup/transfer)

src/ → domain services + worker projects (microservices architecture)

infra/docker/camunda/ → Camunda 8 stack + Postgres + APISIX + etcd (local infra)

frontend/ganipay-mobile/ → React Native (Expo) client

GaniPay.AppHost/ & GaniPay.ServiceDefaults/ → Aspire orchestration setup

Helper scripts: bootstrap.ps1, set-ports.ps1, run-aspire.ps1

Repository Layout
.
├─ src/                          # Domain services + workers (microservices)
├─ bpmn/                         # Camunda BPMN models (register/login/topup/transfer)
├─ GaniPay.Workflow.API/         # Workflow orchestration API
├─ GaniPay.AppHost/              # Aspire app host (local orchestration)
├─ GaniPay.ServiceDefaults/      # Aspire defaults / shared configs
├─ infra/
│  └─ docker/
│     └─ camunda/                # Docker Compose: Camunda 8, Postgres, APISIX, etcd, services
└─ frontend/
   └─ ganipay-mobile/            # React Native (Expo) mobile app

DDD Bounded Contexts

The solution is organized around the following bounded contexts (generated/managed via scripts):

Customer

Identity

Accounting (ledger & balance logic foundation)

Payments (top-up/transfer orchestration at payment layer)

Expense

TransactionLimit

Integration

Notification

Each context follows a layered DDD layout:

API → HTTP endpoints, middleware, controllers, auth boundary

Application → use-cases, DTOs, mappings (AutoMapper), orchestration per domain

Domain → entities/aggregates, value objects, business rules, domain events

Infrastructure → EF Core persistence, repositories, external clients, integrations

Workflow Orchestration (Camunda 8 / Zeebe)

Workflows are modeled in bpmn/ and executed on Camunda 8.

Why workflow-first?

Flows are visible and debuggable (Operate)

Retry/timeout handling belongs to orchestration, not ad-hoc code paths

Business steps are explicit and auditable

Core process IDs (configured in infra compose):

register

login

topup

transfer

Camunda stack (Docker):

Zeebe Broker

Operate

Tasklist

Elasticsearch (Camunda secondary storage)

Worker Layer

Workers subscribe to Zeebe jobTypes and execute business steps:

Read Zeebe variables

Validate + transform payloads

Call domain APIs

Publish results back to workflow boundary endpoints

Why workers?

Decouples orchestration from business execution

Makes the system retry-safe and failure-tolerant

Allows independent scaling of hot paths (e.g., top-up workers)

Key practices implemented/targeted:

Correlation ID propagation

Retry-safe design (idempotency-ready)

Clear separation: workflow step ≠ domain business logic

API Gateway (APISIX + etcd)

APISIX is the single entrypoint and the place for cross-cutting concerns.

What APISIX provides for GaniPay:

Centralized routing (/workflow-api, /identity-api, /customer-api, etc.)

CORS policy management for the mobile app

Future-ready gateway features:

rate limiting

JWT verification at the edge

IP allowlists / WAF patterns

canary releases, circuit breakers

How routing works (typical approach):

Create APISIX routes that forward paths to internal services

Configure CORS once at the gateway (instead of every microservice)

APISIX Admin API quick idea (pattern used):

APISIX Admin listens on 9180

Gateway traffic is served on 9080

Routes are created via Admin API using X-API-KEY

etcd is used as APISIX config store

In this project, gateway configuration and CORS are handled in the infra scripts/config so mobile/web clients can call a single base URL.

Data Layer (PostgreSQL + DBeaver)

PostgreSQL runs as local infra (Docker)

Microservices are designed for domain isolation and clean persistence boundaries

DBeaver is used for DB inspection (tables, migrations, data verification)

Redis (Caching & Ephemeral State Strategy)

Redis is included in the architecture plan (and can be added to infra) for:

Session caching / token-related ephemeral data

Rate limiting counters (gateway and/or app layer)

OTP/verification short-lived states

Correlation/result caching (short-lived workflow result windows)

Distributed locking for sensitive money-movement operations (where needed)

Patterns:

Cache-aside for read-heavy lookups

TTL-based ephemeral storage for flows

Locking only where absolutely required

Local Orchestration Options (Aspire vs Docker Compose)

This repository supports two productive development modes:

1) Aspire (developer-first)

Fast local iteration

Service discovery & dashboards

Great for inner-loop dev

2) Docker Compose (production-like)

Camunda + gateway + DB + services in a real container network

Mirrors production deployment concerns

Best for end-to-end workflow testing

Getting Started
Prerequisites

.NET SDK (matching solution version)

Docker Desktop

Node.js + npm (for mobile app)

(Optional) DBeaver, Postman

Run with Aspire

From repo root:

.\run-aspire.ps1

Run Full Stack with Docker Compose
cd .\infra\docker\camunda
docker compose up -d


This brings up:

Camunda 8 stack (Zeebe + Operate + Tasklist + Elasticsearch)

Postgres

APISIX + etcd

Selected APIs/workers (depending on compose config)

Run the Mobile App (Expo)
cd frontend/ganipay-mobile
npm install
npm run start

Ports
Local domain API ports (launchSettings via script)

From set-ports.ps1:

Customer 5101

Identity 5102

Accounting 5103

Expense 5104

TransactionLimit 5105

Payments 5106

Integration 5107

Notification 5108

Docker compose examples

Camunda Operate/Tasklist UI: 8081

Workflow API: 5210

Identity: 5211

Customer: 5212

Accounting: 5213

Payments: 5214

APISIX: 9080 / Admin: 9180

Postgres: 5432

Elasticsearch: 9200

Testing & Tooling

Swagger per API for contract testing

Postman for flow simulation and regression checks

DBeaver for DB inspection and verification

Camunda Operate for process/job inspection

APISIX Admin API for dynamic route configuration and gateway debugging

Observability & Operations

Camunda Operate for workflow-level visibility

Gateway-level logging strategy via APISIX (extensible)

OpenTelemetry-ready perspective for tracing (roadmap)

Correlation ID propagation is treated as a first-class concern

Security Notes

This is a production-minded MVP; real production hardening requires:

Secret management (Vault/KeyVault), no secrets in repo

TLS everywhere + network policies

Strict authn/authz boundaries

Rate limits, audit logs, tamper-resistant logging

Idempotency guarantees across workers/APIs for financial operations

Kubernetes Strategy (k3d / AKS-ready)

GaniPay is designed to move from local containers to Kubernetes:

One deployment per service

Separate scaling for workers vs APIs

Gateway (APISIX) as Ingress / north-south entry

ConfigMaps/Secrets for environment management

Production path: k3d (dev) → AKS (prod)

Roadmap

Per-service DB isolation & migrations pipeline improvements

Redis integration in infra (compose + app usage)

Kubernetes manifests / Helm charts (k3d/AKS)

Full OpenTelemetry tracing standardization

Stronger gateway policies: JWT at edge, rate limiting, WAF patterns
