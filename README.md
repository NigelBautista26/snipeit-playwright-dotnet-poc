# Snipe-IT Playwright .NET POC

Small Playwright + .NET/NUnit assessment project for the Snipe-IT demo.

The framework follows the same functional POM principles as the Sandfly Playwright framework:

```text
Constants = fixed values and paths
Pages     = selectors and reusable browser actions
Tests     = scenarios, test steps and assertions
Utils     = shared configuration, data and cleanup helpers
```

The project deliberately avoids custom fixtures, page classes, generic click wrappers, service containers and unnecessary framework layers.

`PageTest` is Playwright .NET's official NUnit base and simply supplies `Page`/`Context`, similar to Playwright Test supplying `{ page }` in TypeScript. It is not a custom framework fixture.

## Structure

```text
SnipeIt.Automation/
├── Constants/
├── Models/
├── Pages/
├── Tests/
│   ├── Api/
│   ├── Functional/
│   ├── Visual/
│   └── E2E/
└── Utils/
```

## Test layers

The suite separates backend behaviour, individual UI behaviour, appearance and the complete user journey. Each layer answers a different question.

| Layer      | Main question                                | Scope                                                         |
| ---------- | -------------------------------------------- | ------------------------------------------------------------- |
| API        | Does the backend return the expected result? | Direct API requests and responses                             |
| Functional | Does this part of the form behave correctly? | Individual browser interactions and form states               |
| Visual     | Does the form look as approved?              | Screenshot comparison against an approved baseline            |
| E2E        | Can the user complete the whole workflow?    | Connected journey through creation, checkout and verification |

### API

Three focused backend checks support the assessed feature.

These tests call the application API directly and assert the relevant responses. They test backend behaviour without navigating through the browser.

This layer helps distinguish backend problems from browser interaction or rendering problems. It does not verify that users can operate the UI or that the page looks correct.

### Functional

These tests exercise focused asset-form behaviour in the browser without saving an asset.

The emphasis is on individual interactions and the resulting form state, such as how controls respond when a selection changes and whether the expected fields are available.

This layer checks the form while the user is completing it. Because these tests do not submit the form, they do not prove that an asset can be saved or that the backend persists its values.

### Visual

The visual test captures the Ready-to-Deploy checkout form state and compares it with an approved screenshot baseline.

This layer detects changes in appearance, including layout, spacing, alignment and visible controls. A form can behave correctly while still having a visual regression.

The test requires an approved platform/browser-specific baseline. Creating the first baseline establishes the reference; it does not prove that the appearance is correct until that reference has been reviewed.

### E2E

The E2E test covers the requested journey:

```text
Create asset → Check out → Find in list → Open details → Verify history
```

This layer checks that the steps work together and that the resulting asset state is reflected across the application.

It provides confidence in the complete workflow, including saving and subsequent verification. It does not attempt to exercise every form variation or reproduce all functional and visual checks.

### Why use separate layers?

A single E2E test cannot efficiently cover every form interaction, backend response and visual detail.

Separating these concerns makes failures easier to investigate:

- An **API failure** points towards a backend response or data problem.
- A **functional failure** points towards a specific form interaction or state.
- A **visual failure** identifies an appearance difference that needs review.
- An **E2E failure** shows that the complete workflow could not be completed or verified.

The layers intentionally avoid repeating the same detailed assertions. Some navigation and setup may overlap because multiple tests need to reach the same form, but each layer has a different verification purpose.

## Configuration

Playwright runner configuration is in `runsettings.runsettings`.

Application/environment configuration is in `Utils/TestConfig.cs` and can be overridden with:

- `SNIPEIT_BASE_URL`
- `SNIPEIT_USERNAME`
- `SNIPEIT_PASSWORD`
- `CLEANUP`
- `RANDOM_SEED`

The public demo defaults to its published `admin` / `password` account.

## Run

```bash
# first-time setup
dotnet restore --locked-mode
dotnet build
pwsh SnipeIt.Automation/bin/Debug/net10.0/playwright.ps1 install chromium

# individual layers
HEADLESS=false bash scripts/test.sh e2e
HEADLESS=false bash scripts/test.sh functional
HEADLESS=false bash scripts/test.sh api
HEADLESS=false bash scripts/test.sh visual

# complete suite
bash scripts/test.sh all
```

The visual test needs an approved platform/browser-specific baseline before it can pass as a true comparison.
