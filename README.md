# Snipe-IT Playwright .NET POC

Small Playwright + .NET/NUnit assessment project for the Snipe-IT demo.

The framework follows the same functional POM principles as the Sandfly Playwright framework:

```text
Constants = fixed values and paths
Pages     = selectors and reusable browser actions
Tests     = scenarios, test steps and assertions
Utils     = shared configuration, data and cleanup helpers
```

Pages use static functions that receive an `IPage`. The project deliberately avoids custom fixtures, instance-based page objects, generic click wrappers, service containers and unnecessary framework layers.

`PageTest` is Playwright .NET's official NUnit base and supplies `Page`/`Context`, similar to Playwright Test supplying `{ page }` in TypeScript. It is not a custom framework fixture.

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
| API        | Does the backend return the expected result? | API requests and responses after browser authentication       |
| Functional | Does this part of the form behave correctly? | Individual browser interactions and form states               |
| Visual     | Does the form look as approved?              | Screenshot comparison against an approved baseline            |
| E2E        | Can the user complete the whole workflow?    | Connected journey through creation, checkout and verification |

### API

Two API tests support the assessed feature:

- **ReadyToDeployStatusSupportsCheckout** finds the Ready to Deploy status, verifies its type is deployable and checks that its checkout-eligibility endpoint returns the expected result.
- **NewGeneratedAssetTagDoesNotAlreadyExist** searches for a newly generated asset tag and verifies that the API returns no matching assets.

Each test authenticates through the browser first. API requests then use the browser context's authenticated session and the application's CSRF token.

The assertions focus on backend responses. They do not verify the form's appearance or the complete asset-creation workflow.

### Functional

Two functional tests exercise asset-form behaviour in the browser without creating an asset:

- **AssetTagIsRequiredBeforeCreationCanBeSubmitted** clears the asset tag, clicks Save, checks that the field has the required attribute and verifies that the browser remains on the creation page.
- **ReadyToDeployAllowsUserCheckoutAndArchivedDoesNot** checks that the user selector is visible for Ready to Deploy, hidden for Archived and visible again after switching back. It also selects a random user and verifies the selected user ID and checkout-to-user radio state.

This layer checks focused form interactions. It does not prove that valid form values are saved or persisted by the backend.

### Visual

**ReadyToDeployCheckoutFormMatchesApprovedSnapshot** fills the asset form with fixed values, selects Ready to Deploy and compares a screenshot of the form with an approved baseline.

The capture uses a fixed viewport, waits for a stable image and disables animations. Model and user dropdowns are masked, and dynamic custom fields are hidden to reduce differences caused by shared demo data.

This layer detects appearance changes in the captured area, including layout, spacing, alignment and visible controls. Masked and hidden areas are outside its visual coverage.

The committed baseline is for **macOS ARM64 with Chromium**. Other operating systems, architectures or browsers require their own reviewed baseline.

A missing baseline produces a received image for review. Creating or accepting a baseline establishes the reference; it is not a successful visual comparison until that reference has been reviewed and the test rerun.

### E2E

**CreatedMacBookIsListedWithCorrectDetailsAndCheckoutHistory** covers the requested journey:

```text
Log in → Create and check out → Find in list → Open details → Verify history
```

The test:

1. Logs in to the Snipe-IT demo.
2. Creates a Macbook Pro 13" asset with Ready to Deploy status and checks it out to a randomly selected user.
3. Searches the asset list using its unique generated tag and verifies that exactly one matching row exists.
4. Opens the asset from the list and verifies its tag, serial, model, status and assigned user.
5. Opens History and verifies the creation event, checkout event and checkout target user.
6. Deletes the generated asset when cleanup is enabled.

This layer checks that the workflow works across multiple pages and that the saved asset state is reflected in its details and history.

It does not attempt to exercise every form variation or repeat all functional and visual assertions.

### Why use separate layers?

A single E2E test cannot efficiently cover every form interaction, backend response and visual detail.

Separating these concerns makes failures easier to investigate:

- An **API failure** can identify a backend response, data or authentication problem.
- A **functional failure** identifies a specific form interaction or state that needs investigation.
- A **visual failure** identifies an appearance difference that needs review.
- An **E2E failure** shows that the complete workflow could not be completed or verified.

The layers intentionally avoid repeating the same detailed assertions. Navigation and setup may overlap because multiple tests need to reach the same form, but each layer has a different verification purpose.

## Prerequisites

- .NET SDK **10.0.403**, as specified in `global.json`. Its roll-forward policy permits later patches within the same SDK feature band.
- PowerShell 7 (`pwsh`), used to install Playwright browsers.
- Bash, used by `scripts/test.sh`. On Windows, run these commands in Git Bash or another suitable Bash environment.
- Git, to clone the repository.

Run all setup and test commands from the repository root.

## Configuration

Playwright runner configuration is in `runsettings.runsettings`.

Application/environment configuration is in `SnipeIt.Automation/Utils/TestConfig.cs` and can be overridden with:

| Variable           | Purpose                                                                       |
| ------------------ | ----------------------------------------------------------------------------- |
| `SNIPEIT_BASE_URL` | Application origin, such as `https://demo.snipeitapp.com`                     |
| `SNIPEIT_USERNAME` | Login username                                                                |
| `SNIPEIT_PASSWORD` | Login password                                                                |
| `CLEANUP`          | Whether to delete the generated E2E asset; defaults to `true`                 |
| `RANDOM_SEED`      | Positive integer controlling random user selection from the available options |

The public demo defaults to its published `admin` / `password` account. Credentials must be provided when targeting a non-demo instance.

The test script also supports:

| Variable   | Purpose                                                             |
| ---------- | ------------------------------------------------------------------- |
| `HEADLESS` | Whether to run without a visible browser window; defaults to `true` |
| `BROWSER`  | `chromium`, `firefox` or `webkit`; defaults to `chromium`           |

The committed visual baseline covers macOS ARM64 with Chromium only.

## Run

Clone the repository and enter its root directory:

```bash
git clone https://github.com/NigelBautista26/snipeit-playwright-dotnet-poc.git
cd snipeit-playwright-dotnet-poc
```

For explicit first-time setup:

```bash
dotnet restore --locked-mode
dotnet build --no-restore
pwsh SnipeIt.Automation/bin/Debug/net10.0/playwright.ps1 install chromium
```

`dotnet restore --locked-mode` restores NuGet dependencies using the committed `packages.lock.json`. The Playwright installation command downloads Chromium.

Run individual layers with a visible browser:

```bash
HEADLESS=false bash scripts/test.sh e2e
HEADLESS=false bash scripts/test.sh functional
HEADLESS=false bash scripts/test.sh api
HEADLESS=false bash scripts/test.sh visual
```

Run the complete suite in headless mode:

```bash
bash scripts/test.sh all
```

The test script automatically restores dependencies, builds the project and installs the selected browser before running the requested tests.

TRX test results are written to `.output/results/`.

## Execution notes

- The public demo is shared and periodically resets its database. A reset or changes to demo data can interrupt a run.
- E2E assets use unique generated tags. Cleanup is enabled by default and verifies the generated tag and serial before deletion.
- Set `CLEANUP=false` to retain the created asset for manual inspection.
- Visual tests require an approved baseline matching the operating system, architecture and browser used for the run.
