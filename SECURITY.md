# Security Policy

## About This Policy

FinSight is a portfolio application demonstrating secure financial-data aggregation, transaction processing, financial intelligence, and alerting using modern .NET application architecture.

Although FinSight is currently a portfolio project and uses mock banking providers rather than real financial institutions, security is treated as a first-class engineering concern. The project includes authentication, authorization, ownership isolation, token management, audit/security events, input validation, transactional processing, background workers, observability, and protected configuration.

This policy explains which versions are supported, what security issues are considered vulnerabilities, how to report them, and how security issues are handled.

---

## Supported Versions

FinSight is currently developed as an actively evolving portfolio project. There are not yet multiple stable public release lines requiring long-term parallel security support.

Security fixes are therefore focused on the actively maintained development line.

| Version / Branch                          | Supported          |
| ----------------------------------------- | ------------------ |
| `dev` — current development version       | :white_check_mark: |
| `staging`                                 | :white_check_mark: |
| `main` — current released/stable snapshot | :white_check_mark: |
| Older commits / historical snapshots      | :x:                |

### Support policy

Security fixes are applied to the current maintained codebase.

Older historical commits, experimental branches, abandoned feature branches, and superseded implementations are not guaranteed to receive security updates.

When a security issue affects the current architecture, the remediation may be applied to the active development branch first and then promoted through the normal `dev` → `staging` → `main` workflow.

Because FinSight is an evolving portfolio project, the supported version policy may change when formal releases and versioned release tags are introduced.

---

## Scope

Security reports are relevant to the FinSight application and its supporting code, including:

* ASP.NET Core API endpoints
* Authentication and authorization
* ASP.NET Core Identity
* JWT access tokens
* Refresh-token rotation and revocation
* User lifecycle and account security
* User ownership and tenant/data isolation
* Financial-account access controls
* Transaction APIs and persistence
* Mock banking integrations
* RabbitMQ messaging
* Background workers
* Transactional outbox processing
* Database access and Entity Framework Core
* PostgreSQL persistence
* Redis-backed infrastructure
* Input validation and request handling
* Security and audit events
* Secrets and application configuration
* Docker and container configuration
* CI/CD workflows
* Logging and observability where sensitive-data exposure is possible
* Dependencies that introduce exploitable security vulnerabilities

---

## Out of Scope

The following are generally outside the scope of this security policy unless they demonstrate an actual security impact on FinSight:

### The mock nature of banking integrations

FinSight currently uses mock banking providers for portfolio and development purposes.

A simulated bank response, intentionally fake account, synthetic transaction, or other mock-provider behavior is not itself a security vulnerability.

### Application features that are intentionally incomplete

A feature that has not yet been implemented is not considered a vulnerability merely because it is absent.

### Cosmetic or non-security defects

Examples include:

* UI bugs with no security impact
* Incorrect formatting
* Non-sensitive error messages
* Performance issues that do not create a practical security consequence
* Expected validation failures
* Feature requests
* General code-quality suggestions without a security impact

### Social engineering

Reports involving attacks against FinSight maintainers, contributors, or third-party services outside the application's control are outside this policy.

### Denial-of-service testing against public infrastructure

Do not perform destructive or high-volume availability testing against hosted environments without explicit authorization.

---

## Security Model

FinSight follows a layered security model:

```text
Authentication
      ↓
Authorization
      ↓
Ownership Isolation
      ↓
Application / Domain Rules
      ↓
Persistence
```

### Authentication

FinSight uses ASP.NET Core Identity together with token-based authentication.

Authentication establishes the identity of the caller before protected application functionality is executed.

### Authorization

Authentication alone does not grant access to every resource.

Protected operations must enforce application authorization rules appropriate to the authenticated user and the requested resource.

### Ownership isolation

Financial data is treated as user-owned data.

An authenticated user must not be able to access another user's:

* Financial accounts
* Account connections
* Transactions
* Subscriptions
* Anomalies
* Insights
* Notifications
* Other user-owned financial intelligence

Resource ownership therefore forms an explicit security boundary rather than relying solely on authentication.

### Auditability

Security-sensitive operations and relevant security events are designed to be auditable through the application's security/audit mechanisms.

---

## Expected Security Controls

FinSight is designed with the following security controls:

### Identity and credentials

* ASP.NET Core Identity for user management
* Secure password handling through the Identity framework
* Token-based authentication
* Access-token expiration
* Refresh-token rotation
* Refresh-token revocation
* Protected authentication endpoints
* Explicit account/user lifecycle states

### Authorization

* Authenticated endpoint protection
* Role/permission-aware authorization where applicable
* Resource ownership validation
* User-context abstraction for application services
* Separation between authentication and resource authorization

### Data protection

Sensitive information should not be exposed through:

* API responses
* Exception details
* Application logs
* Audit records
* Message payloads
* Configuration files
* Source control

Financial data should only be returned to an authenticated and authorized owner.

### Infrastructure

FinSight uses infrastructure components including:

* PostgreSQL
* Redis
* RabbitMQ
* Docker
* Background workers
* OpenTelemetry / structured observability
* Serilog

These components must be configured using secure credentials and environment-specific configuration rather than hard-coded secrets.

### Messaging

Asynchronous financial processing must preserve security boundaries.

Messages and events should not grant a consumer implicit access to data owned by another user.

Consumers must treat event payloads as untrusted application input and preserve the same authorization and data-isolation assumptions as synchronous application flows.

### Database security

Database access should use:

* Parameterized database operations through Entity Framework Core
* Application-level ownership constraints
* Proper database credentials
* Environment-specific configuration
* Least-privilege database access where practical

Raw SQL or database-level operations must not introduce SQL injection vulnerabilities.

---

## Reporting a Vulnerability

Please do **not** report a suspected security vulnerability through a public GitHub issue.

Public disclosure before a fix is available can unnecessarily expose users, contributors, or deployments to exploitation.

### Preferred reporting method

Please use a **private GitHub Security Advisory** for the FinSight repository when that feature is available.

When submitting a report, include enough information for the issue to be reproduced and assessed.

A useful report should contain:

* A clear description of the vulnerability
* The affected component, endpoint, class, workflow, or dependency
* The affected branch, version, or commit
* Steps to reproduce the issue
* Expected behavior
* Actual behavior
* Security impact
* Any relevant request/response examples
* Proof-of-concept code or screenshots where appropriate
* A suggested mitigation, if you have one

Please avoid including real credentials, real financial information, authentication tokens, API keys, or other sensitive personal information in the report.

Use synthetic or redacted data whenever possible.

---

## What to Expect After Reporting

Security reports are reviewed privately.

The maintainer will attempt to:

1. Confirm receipt of the report.
2. Determine whether the issue is reproducible.
3. Assess its severity and potential impact.
4. Identify affected components and versions.
5. Develop and test an appropriate fix.
6. Apply the fix through the project's normal development workflow.
7. Document the remediation where appropriate.
8. Coordinate disclosure after the vulnerability has been reasonably addressed.

Response and remediation times may vary because FinSight is maintained as a portfolio project rather than as a continuously staffed commercial security operation.

Reports containing clear reproduction steps and demonstrated security impact will generally receive the highest priority.

---

## Vulnerability Severity

Security issues are assessed according to practical impact rather than only by how technically interesting the issue is.

Particular priority is given to vulnerabilities that could result in:

### Critical impact

* Remote code execution
* Complete authentication bypass
* Arbitrary access to all users' financial data
* Credential or secret compromise with broad application impact
* Complete compromise of a production deployment

### High impact

* Unauthorized access to another user's financial data
* Authentication or refresh-token compromise
* Privilege escalation
* Significant authorization bypass
* SQL injection
* Sensitive secret disclosure
* Persistent compromise of application infrastructure

### Medium impact

* Limited unauthorized data access
* Security-control bypass with restricted scope
* Sensitive information exposure without direct account compromise
* Significant weaknesses requiring additional conditions to exploit

### Low impact

* Minor information disclosure
* Defense-in-depth issues
* Security hardening opportunities with limited practical exploitability

Severity may be adjusted based on exploitability, required privileges, attack complexity, affected users, and actual business/security impact.

---

## Responsible Disclosure

FinSight follows a responsible-disclosure approach.

Security researchers are asked to:

* Report vulnerabilities privately.
* Give the maintainer a reasonable opportunity to investigate and remediate the issue.
* Avoid accessing, modifying, deleting, or exposing data belonging to other users.
* Avoid intentionally disrupting availability.
* Avoid persistence after demonstrating the vulnerability.
* Stop testing once sufficient evidence has been collected.

Security research performed in good faith and in accordance with this policy is appreciated.

---

## Safe Testing Guidelines

When testing FinSight, use:

* Local development environments
* Synthetic users
* Synthetic financial accounts
* Synthetic transactions
* Mock banking providers
* Non-production credentials
* Isolated test infrastructure

Do not use real banking credentials, real financial-account data, real payment information, or another person's personal data.

Do not attempt to compromise systems or services that FinSight does not own or operate.

Do not perform destructive tests against shared infrastructure.

---

## Secrets and Sensitive Information

Never commit the following to source control:

* Passwords
* JWT signing keys
* Refresh-token secrets
* Database credentials
* RabbitMQ credentials
* Redis credentials
* API keys
* Cloud credentials
* Private certificates
* Private keys
* Production connection strings
* Real banking credentials
* Real personal or financial data

Local and deployment secrets should be supplied through appropriate environment-specific configuration or secret-management mechanisms.

If a secret is accidentally committed, removing it from the latest commit is **not** sufficient. The exposed credential should be considered compromised and rotated/revoked as appropriate.

---

## Dependency Vulnerabilities

FinSight relies on the .NET ecosystem and third-party infrastructure libraries.

Dependency vulnerabilities should be evaluated based on:

* Whether the vulnerable package is actually used
* Whether the vulnerable component is reachable
* Whether the affected functionality is enabled
* Exploitability in the FinSight deployment model
* Availability of a patched version

Security-related dependency updates should be prioritized and validated before promotion to the stable branch.

Automated dependency/security checks in CI/CD should be treated as part of the project's security controls rather than as a replacement for manual vulnerability assessment.

---

## Logging and Sensitive Data

Logs are operational and security-sensitive artifacts.

Application logging must not expose:

* Passwords
* Access tokens
* Refresh tokens
* Authentication secrets
* Database credentials
* API keys
* Complete sensitive financial records
* Other secrets that could enable account or infrastructure compromise

Structured logging and observability should provide enough contextual information to investigate failures and security events without unnecessarily duplicating sensitive application data.

---

## Security Issues in Financial Intelligence

FinSight processes financial information such as:

* Financial accounts
* Transactions
* Spending categories
* Subscription detections
* Anomaly detections
* Financial insights
* Notifications

These features must preserve the same ownership boundary as the underlying account and transaction data.

For example, an anomaly or subscription derived from User A's transactions must never be exposed through User B's API responses, events, notifications, or background processing.

The intelligence pipeline must therefore be treated as a data-propagation boundary: derived financial information is subject to the same ownership and confidentiality requirements as its source data.

---

## Security of AI-Generated Results

FinSight includes AI-assisted financial transaction categorization and financial intelligence processing.

AI-generated output must not be treated as inherently trusted input.

Security-sensitive application behavior must not rely solely on an LLM response being correct or well-formed.

AI output should therefore be:

* Validated against application expectations
* Constrained by explicit application rules
* Parsed using structured contracts where applicable
* Handled defensively when malformed
* Prevented from bypassing authorization or ownership rules

AI-generated categorization or financial insight must never override application-level security controls.

---

## Security Updates

Security-related changes may include:

* Authentication fixes
* Authorization fixes
* Ownership-isolation fixes
* Token-security improvements
* Dependency updates
* Secret-handling improvements
* Infrastructure hardening
* Input-validation fixes
* Data-leak prevention
* Logging/privacy improvements

Security fixes should be tested before promotion through the normal FinSight branch workflow:

```text
dev
 ↓
staging
 ↓
main
```

Where practical, security regressions should be accompanied by automated tests that prevent the vulnerability from being reintroduced.

---

## Security Contact

For vulnerabilities, use the repository's private security-reporting mechanism rather than a public issue.

Do not publish exploit details publicly until the issue has been reviewed and a reasonable remediation path has been established.

---

## Disclaimer

FinSight is a portfolio project and is not currently presented as a production banking platform or as a regulated financial service.

The current implementation uses mock banking providers for demonstration and development purposes.

This security policy describes the project's intended engineering and disclosure practices; it does not constitute a guarantee of security or suitability for processing real financial information in an uncontrolled environment.

Any production deployment would require additional security, privacy, compliance, infrastructure, operational, and financial-industry controls beyond the scope of this portfolio project.
