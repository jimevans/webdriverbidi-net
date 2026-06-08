# bidi-cli

A command-line interface for automating a browser via the WebDriver BiDi protocol, built on top of the `WebDriverBiDi` and `WebDriverBiDi.Client` libraries. Designed to be called by an agent or script: one command per invocation, JSON on stdout, a numeric exit code.

## Building

```sh
dotnet build src/WebDriverBiDi.Cli
```

The output binary is named `bidi-cli`. To run directly from the repo without installing:

```sh
dotnet run --project src/WebDriverBiDi.Cli -- <verb> [options]
```

## Usage

```
bidi-cli <verb> [options]
```

Every invocation requires a **connection flag** that tells the CLI how to reach a browser.

## Connection flags

Exactly one of these must be present on every invocation (except `launch`, which uses `--browser` differently — see below).

| Flag | Description |
|---|---|
| `--ws <url>` | Connect to an already-running browser via its WebSocket URL. The caller owns the browser process; `bidi-cli` leaves it running after the command completes. |
| `--browser <chrome\|firefox>` | Launch a new browser, execute the command, then quit the browser. Useful for one-shot scripts. |
| `--headless` | Run the browser without a visible window. Only meaningful together with `--browser`. |

### Getting the WebSocket URL from a running browser

The preferred way is to use the `launch` verb (see below), which starts a browser and prints its WebSocket URL for use with subsequent `--ws` calls.

Alternatively, for **Chrome or Edge**, start the browser manually with the `--remote-debugging-port` flag:

```sh
google-chrome --remote-debugging-port=9222
```

Then fetch the WebSocket URL from the browser's debug endpoint:

```sh
curl -s http://localhost:9222/json/version | jq -r '.webSocketDebuggerUrl'
```

For **Firefox**, start `geckodriver` and use the WebSocket URL it prints on startup.

## Common options

These apply to every verb.

| Flag | Default | Description |
|---|---|---|
| `--context <id>` | first context in tree | Browsing context ID to target. Obtain IDs from the browser's context tree. Useful when multiple tabs are open. |
| `--frame <css-selector>` | none | Target elements inside a frame. The selector matches an `<iframe>` element in the top-level context; subsequent locator flags search inside that frame's document. |
| `--user-context <id>` | none | Open a new tab inside the specified user context (isolated browser profile). Only meaningful with `--ws`. Use `create-user-context` to obtain an ID. |
| `--timeout <seconds>` | `30` | Seconds to wait for element resolution and command completion before giving up. |
| `--pretty` | off | Pretty-print the JSON output. Useful for manual inspection. |

## Output format

Every command prints a single JSON object to **stdout** and exits.

**Success:**
```json
{"ok": true, ...command-specific fields...}
```

**Failure:**
```json
{"ok": false, "error": "description of what went wrong"}
```

Errors also go to **stderr** so stdout can always be parsed as JSON regardless of outcome. Exit codes:

| Code | Meaning |
|---|---|
| `0` | Command succeeded |
| `1` | Runtime error (element not found, navigation failed, script exception, timeout) |
| `2` | Usage error (unknown verb, missing required flag, conflicting flags) |

## Locator flags

Element commands require exactly one locator flag.

| Flag | Strategy |
|---|---|
| `--css <selector>` | CSS selector, e.g. `"input[name=email]"` |
| `--xpath <expression>` | XPath expression, e.g. `"//button[text()='Submit']"` |
| `--text <string>` | Inner-text match, e.g. `"Sign in"` |

When a locator matches more than one element, the command targets the first match by default. Use `--nth <n>` (zero-based) to target a specific match.

## Verbs

### `launch`

Starts a browser and immediately prints its WebSocket URL, then exits — leaving the browser running. Pass the returned `ws` value as `--ws` to all subsequent commands. This is the recommended way to start a persistent browser session.

**Required:** `--browser <chrome|firefox>`

**Optional launch flags** (only apply to this verb):

| Flag | Description |
|---|---|
| `--channel <stable\|beta\|dev\|alpha>` | Browser release channel. Defaults to `stable`. `dev` is the developer preview channel; `alpha` is Canary (Chrome) or Nightly (Firefox). |
| `--browser-path <path>` | Full path to a specific browser executable. Mutually exclusive with `--system-browser`. |
| `--system-browser` | Use the browser already installed at its default system location. Mutually exclusive with `--browser-path`. |
| `--port <n>` | Port for the browser's debug endpoint. Defaults to a random free port. |
| `--launch-via driver` | Launch through a WebDriver Classic driver executable (chromedriver / geckodriver). The driver is auto-downloaded if not found. |

When neither `--browser-path` nor `--system-browser` is given, the browser is automatically located or downloaded to a local cache (`~/.cache/webdriverbidi-net`).

```sh
# Launch headless Chrome, auto-download if needed
bidi-cli launch --browser chrome --headless
```

```json
{"ok": true, "ws": "ws://localhost:54321/devtools/browser/...", "pid": 12345}
```

```sh
# Launch Firefox beta via geckodriver
bidi-cli launch --browser firefox --channel beta --launch-via driver
```

```json
{"ok": true, "ws": "ws://localhost:4444/session/...", "pid": 0}
```

`pid` is the browser process ID, or `null` when not applicable (e.g. remote grid or some driver-based launches).

---

### `close`

Sends `browser.close` over the BiDi session, which terminates all browsing contexts and the browser process. Use this to shut down a browser started by `launch`.

**Required:** `--ws <url>`

```sh
WS=$(bidi-cli launch --browser chrome --headless | jq -r '.ws')
# ... do work ...
bidi-cli close --ws "$WS"
```

```json
{"ok": true}
```

**Note:** `browser.close` is a WebDriver BiDi protocol command, not an OS kill signal. It works reliably for Chrome and Edge launched directly or via a driver. For Firefox launched via geckodriver, the BiDi close command may not be honoured by the driver; if `close` returns an error, use the `pid` from `launch` to kill the process directly:

```sh
kill "$PID"   # Linux / macOS
```

---

### `navigate`

Navigate the page to a URL and wait for it to finish loading.

**Required:** `--url <url>`

```sh
bidi-cli navigate --ws ws://localhost:9222/... --url https://example.com
```

```json
{"ok": true, "url": "https://example.com/"}
```

The returned `url` reflects the final URL after any redirects.

---

### `click`

Click an element.

**Required:** a locator flag

```sh
bidi-cli navigate --ws ws://... --css "button[type=submit]"
```

```json
{"ok": true}
```

---

### `type`

Type text into an element (focuses first, then sends key events character by character).

**Required:** a locator flag, `--value <text>`

```sh
bidi-cli type --ws ws://... --css "input[name=email]" --value "user@example.com"
```

```json
{"ok": true}
```

---

### `clear`

Clear the current value of an input or textarea element.

**Required:** a locator flag

```sh
bidi-cli clear --ws ws://... --css "input[name=search]"
```

```json
{"ok": true}
```

---

### `get-text`

Get the visible text content of an element.

**Required:** a locator flag

```sh
bidi-cli get-text --ws ws://... --css "h1"
```

```json
{"ok": true, "text": "Welcome back"}
```

---

### `get-attr`

Get the value of a named attribute from an element.

**Required:** a locator flag, `--name <attribute>`

```sh
bidi-cli get-attr --ws ws://... --css "a.logo" --name href
```

```json
{"ok": true, "value": "https://example.com/home"}
```

`value` is `null` in the JSON when the attribute is present but has no value, and the `"value"` key is absent when the attribute does not exist on the element.

---

### `check`

Check a checkbox or radio button (no-op if already checked).

**Required:** a locator flag

```sh
bidi-cli check --ws ws://... --css "input[name=agree]"
```

```json
{"ok": true}
```

---

### `uncheck`

Uncheck a checkbox (no-op if already unchecked).

**Required:** a locator flag

```sh
bidi-cli uncheck --ws ws://... --css "input[name=newsletter]"
```

```json
{"ok": true}
```

---

### `select`

Select an option in a `<select>` element. Exactly one selection strategy must be given.

**Required:** a locator flag, plus one of:

| Flag | Selects by |
|---|---|
| `--option-value <value>` | The `value` attribute of the `<option>` |
| `--option-text <text>` | The visible text of the `<option>` |
| `--option-index <n>` | Zero-based position in the list |

```sh
bidi-cli select --ws ws://... --css "select[name=country]" --option-text "United Kingdom"
```

```json
{"ok": true}
```

---

### `hover`

Move the mouse pointer over an element (triggers `:hover` styles and `mouseover` events).

**Required:** a locator flag

```sh
bidi-cli hover --ws ws://... --css "nav > li:nth-child(2)"
```

```json
{"ok": true}
```

---

### `screenshot`

Capture a PNG screenshot of the current viewport.

**Optional:** `--output <path>` — write the PNG to a file. If omitted, the PNG bytes are returned as a base64-encoded string in the JSON.

```sh
# Save to file
bidi-cli screenshot --ws ws://... --output /tmp/page.png
```

```json
{"ok": true, "path": "/tmp/page.png"}
```

```sh
# Inline base64 (e.g. for an agent that processes the image directly)
bidi-cli screenshot --ws ws://...
```

```json
{"ok": true, "data": "iVBORw0KGgo..."}
```

---

### `eval`

Evaluate a JavaScript expression in the current browsing context and return the result.

**Required:** `--script <expression>`

The expression is evaluated with `awaitPromise: true`, so you can return a Promise and the result will be the resolved value.

```sh
bidi-cli eval --ws ws://... --script "document.title"
```

```json
{"ok": true, "result": "Example Domain"}
```

```sh
bidi-cli eval --ws ws://... --script "document.querySelectorAll('a').length"
```

```json
{"ok": true, "result": "3"}
```

The `result` field is always a string. Complex values (objects, arrays, DOM nodes) are represented by their JavaScript type name (e.g. `"Object"`, `"Node"`).

---

### `wait-for-load`

Wait for the next `browsingContext.load` event on the current context. Use this after triggering a navigation indirectly (e.g. by clicking a link) when you need to know the final URL before proceeding.

```sh
bidi-cli wait-for-load --ws ws://...
```

```json
{"ok": true, "url": "https://example.com/dashboard"}
```

---

### `create-user-context`

Creates a new isolated browser profile (user context) and returns its ID. Tabs opened in this context have separate cookies, storage, and cache from the default context and from each other — useful for testing multi-account flows or parallel isolated sessions.

```sh
bidi-cli create-user-context --ws "$WS"
```

```json
{"ok": true, "userContextId": "8f3a2b1c"}
```

---

### `list-user-contexts`

Lists all currently active user contexts, including the built-in `"default"` context.

```sh
bidi-cli list-user-contexts --ws "$WS"
```

```json
{"ok": true, "userContexts": [{"userContextId": "default"}, {"userContextId": "8f3a2b1c"}]}
```

---

### `remove-user-context`

Destroys a user context and closes all browsing contexts (tabs) within it. Cannot be used to remove the `"default"` context.

**Required:** `--user-context <id>`

```sh
bidi-cli remove-user-context --ws "$WS" --user-context "8f3a2b1c"
```

```json
{"ok": true}
```

---

## Frames

Any element command accepts `--frame <css-selector>` to target an element inside an `<iframe>`. The selector matches the `<iframe>` element itself in the top-level document; the subsequent `--css`, `--xpath`, or `--text` locator then searches inside that frame's document.

```sh
# Click a button inside an iframe with id="payment-frame"
bidi-cli click --ws "$WS" --frame "iframe#payment-frame" --css "button.pay-now"

# Get text from an element inside a sandboxed iframe
bidi-cli get-text --ws "$WS" --frame "iframe[name=results]" --css "span.total"
```

Nested frames are not currently supported in a single command. To reach an `<iframe>` inside another `<iframe>`, use `eval` to obtain the inner frame's browsing context ID and then pass it via `--context`.

---

## Agent usage patterns

### Recommended: launch once, then reuse with --ws

Use `launch` to start the browser once and capture its WebSocket URL. All subsequent commands connect to that same browser without re-launching it.

```sh
# Start a headless Chrome and capture the WebSocket URL
WS=$(bidi-cli launch --browser chrome --headless | jq -r '.ws')

bidi-cli navigate      --ws "$WS" --url https://example.com/login
bidi-cli type          --ws "$WS" --css "input[name=username]" --value "alice"
bidi-cli type          --ws "$WS" --css "input[name=password]" --value "s3cr3t"
bidi-cli click         --ws "$WS" --css "button[type=submit]"
bidi-cli wait-for-load --ws "$WS"
bidi-cli get-text      --ws "$WS" --css ".welcome-message"

# Close the browser when done
bidi-cli close --ws "$WS"
```

### Checking outcome in a script

```sh
result=$(bidi-cli get-text --ws "$WS" --css "h1.confirmation")
ok=$(echo "$result" | jq -r '.ok')
if [ "$ok" = "true" ]; then
    echo "Got: $(echo "$result" | jq -r '.text')"
else
    echo "Failed: $(echo "$result" | jq -r '.error')" >&2
    exit 1
fi
```

### One-shot headless run (no persistent browser)

For simple scripts that only need a single command, `--browser` launches a fresh browser, runs the command, and quits — no separate `launch` step needed.

```sh
bidi-cli navigate --browser chrome --headless --url https://example.com
bidi-cli eval     --browser chrome --headless --script "document.title"
```

Each invocation launches and quits its own browser. This is slower than the `launch` + `--ws` approach but requires no state between calls.

### Testing a multi-account flow with user contexts

User contexts provide completely isolated storage — cookies in one context are invisible to another.

```sh
WS=$(bidi-cli launch --browser chrome --headless | jq -r '.ws')

# Create two isolated profiles
CTX_A=$(bidi-cli create-user-context --ws "$WS" | jq -r '.userContextId')
CTX_B=$(bidi-cli create-user-context --ws "$WS" | jq -r '.userContextId')

# Open a tab in each context — they share no cookies or storage
bidi-cli navigate --ws "$WS" --user-context "$CTX_A" --url https://example.com/login
bidi-cli navigate --ws "$WS" --user-context "$CTX_B" --url https://example.com/login

# ... interact with each tab independently via --context <browsing-context-id> ...

# Clean up
bidi-cli remove-user-context --ws "$WS" --user-context "$CTX_A"
bidi-cli remove-user-context --ws "$WS" --user-context "$CTX_B"
bidi-cli close --ws "$WS"
```

### Launching with a specific channel or path

```sh
# Use the system-installed Firefox
WS=$(bidi-cli launch --browser firefox --system-browser | jq -r '.ws')

# Use Chrome Beta, auto-downloaded
WS=$(bidi-cli launch --browser chrome --channel beta | jq -r '.ws')

# Use a specific Chrome binary
WS=$(bidi-cli launch --browser chrome --browser-path /opt/chrome/chrome | jq -r '.ws')
```

## Architecture notes

The project is structured so that a future **persistent server mode** (accepting commands over a WebSocket rather than as CLI arguments) can be added without modifying any command implementations. The key seam is `BrowserSession`: all session-dependent commands receive a `BrowserSession` and a `CommandContext` and know nothing about how the session was established. A server mode would add a third factory path in `BrowserSession.OpenAsync` and a new entry point alongside `Program.cs`.

Frame resolution (`--frame`) is handled entirely in `LocatorParser.ResolveAsync` via `FrameResolver`, which calls `script.callFunction` with the iframe element as `this` to retrieve its `contentWindow` — a `WindowProxyRemoteValue` whose `Value.Context` property is the child browsing context ID. No command implementation is aware of frames directly.

The `launch` command sits outside `BrowserSession` by design: it creates a browser and exits without connecting a `BiDiDriver`, so there is no session to open or close. This is intentional — `launch` is the tool for establishing the long-lived browser process that all subsequent `--ws` calls share.
