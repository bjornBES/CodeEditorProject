# Roadmap

## Phase 1 — Core Editing Loop (Foundations)

**Goal:** Make the editor feel “alive” and predictable.

**Tasks (5–8):**

1. Implement cursor movement commands

   * left / right / up / down
   * clamp correctly at line boundaries
2. Implement basic text insertion

   * via `TextInput(string)`
3. Implement deletion commands

   * backspace
   * delete forward
4. Implement newline insertion

   * split line at cursor
5. Ensure snapshot consistency

   * cursor position always valid
6. Add basic editor invariants

   * no negative positions
   * cursor never past line length
7. (Optional) Add simple status logging for commands

**Exit criteria:**
You can type, move around, and delete text without crashes or weird jumps.

---

### Phase 2 — Selection & Structured Editing

**Goal:** Enable real editing, not just typing.

**Tasks (5–7):**

1. Add selection model to `TextEditor`

   * anchor + active position
2. Implement selection movement

   * shift + arrows
3. Modify insertion to replace selection
4. Modify deletion to delete selection
5. Render selection in snapshots
6. Add select-all command
7. Normalize selection invariants

   * forward/backward selection handling

**Exit criteria:**
User can select text, overwrite it, and delete it naturally.

---

### Phase 3 — Undo / Redo System

**Goal:** Make editing safe and reversible.

**Tasks (6–9):**

1. Define an undo able edit model

   * insert
   * delete
   * replace
2. Record edits per editor or document
3. Implement undo command
4. Implement redo command
5. Merge sequential edits (basic coalescing)
6. Reset redo stack on new edits
7. Verify cursor restoration on undo
8. Snapshot undo state (if needed)
9. Add debug logging for undo stack

**Exit criteria:**
User can confidently edit knowing mistakes are reversible.

---

### Phase 4 — Input, Commands & Keybindings

**Goal:** Separate behavior from input cleanly.

**Tasks (5–8):**

1. Finalize engine-level `KeyEvent` model
2. Replace hardcoded mappings with keybinding table
3. Implement `KeyChord` (key + modifiers)
4. Map `KeyChord → CommandId`
5. Support user-remappable bindings (config file or API)
6. Add default keybinding profile
7. Handle command conflicts deterministically
8. Add fallback behavior for unmapped keys

**Exit criteria:**
User can rebind keys without changing editor logic.

---

### Phase 5 — Files, Documents & Editor Lifecycle

**Goal:** Make it usable across sessions.

**Tasks (5–7):**

1. Clean up open/save semantics

   * always create documents properly
2. Add save command
3. Add “dirty” document tracking
4. Prompt or flag unsaved changes
5. Support multiple open documents
6. Support multiple editors per document
7. Close editor / close document commands

**Exit criteria:**
User can open, edit, save, and manage multiple files.

---

### Phase 6 — Rendering & UX Polish

**Goal:** Make it pleasant to use.

**Tasks (6–9):**

1. Line number rendering
2. Scroll model (vertical at minimum)
3. Cursor blinking
4. Improve text layout (monospace metrics)
5. Handle window resizing
6. Add basic theming (colors, font size)
7. Optimize snapshot diffing (optional)
8. Reduce redraw cost (optional)
9. Visual debug overlay (optional)

**Exit criteria:**
Editor feels responsive and readable for real work.

---

### Phase 7 — Extensibility & Architecture Hardening

**Goal:** Lock in the architecture so it doesn’t rot.

**Tasks (5–8):**

1. Stabilize public engine interfaces
2. Add extension hooks (command registration only)
3. Document core invariants
4. Add command introspection API
5. Add basic test harness (headless)
6. Harden error handling in commands
7. Decouple workspace construction from engine
8. Add sample extension

**Exit criteria:**
The editor can grow without architectural debt exploding.

---

## After All Phases: What a User Can Do

After completing all phases, a user should be able to:

* Open and edit multiple files
* Type, delete, and move the cursor naturally
* Select, replace, and manipulate text
* Undo and redo confidently
* Customize keybindings
* Work with multiple editors on the same document
* Save changes safely
* Use the editor for real coding or writing tasks
* Extend behavior via commands (plugins/extensions)
* Run the editor on different frontends (SDL, Avalonia, etc.)

In short:

> **A real, minimal, extensible code editor — not a toy.**
