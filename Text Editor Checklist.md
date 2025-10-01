# Roadmaps

## MVP Roadmap

### **Phase 1 – Core Foundation**

* [X] New File
* [X] Open File
* [X] Save / Save As
* [X] Close tabs.
* [X] Undo / Redo (multi-level history).
* [X] Cut / Copy / Paste.
* [X] Basic systax highlighing
  * [X] an API for systax highlighing for TextMate
    * [X] C#, Json, Markdown
* [X] Basic themes

---

### **Phase 2 – Productivity Features**

* [X] Basic Find / Replace (in current file).
* [X] Basic Auto-Indent.
  * [X] Make an API for indentation
* [X] Line Numbers (AvaloniaEdit).
* [X] File Explorer
  * [X] Side bars
* [X] Tab Multiple Files handling
  * [X] Pinning tabs
  * [X] Tab overflow handling
  * [X] Get Multiple Tabs working
* [X] Bracket/parenthesis matching
* [X] Define a basic folder structure
* [X] Settings
  * [X] Global User Config
    * [X] Default font size
    * [X] Default font family
    * [X] tab width
      * [X] Tab or Spaces
    * [X] Themes
* [X] Basic Keybindings
  * [X] Make an API for keybindings
  * [X] Keybindings Settings

---

### **Phase 3 – UI Design**

* [X] Design main window layout
  * [X] Menu bar
    * [X] Recent Files / Folders (quick access).
      * [X] Get Sub Sub menus working
  * [X] Tabs layout
  * [X] Status bar (line/col, file info)
* [X] Apply theme colors consistently
* [X] Improve spacing/padding for readability
* [X] Ensure responsive resizing of panels
* [X] Minimal icons for buttons/commands

---

### **Phase 4 – Developer-Friendly Features**

* [X] Code navigation
  * [X] Word wrap toggle
* [X] Custom commands
  * [X] A basic command palette
  * [X] A basic command system

---

### **Phase 5 – Polishing & MVP Release**

* [X] Performance checks
  * [X] Ensure large files open reasonably fast
  * [X] Ensure that the app is fast
* [ ] Cross-platform testing
  * [ ] for Windows and Linux (for now)
* [ ] Packaged builds (MVP is done)
* [ ] Release MVP build (aka launch day for AAA games)

---

### At the end of the MVP a user should be able to

1. Open and edit files
2. Have basic syntax highlighting using Textmate on languages
3. Do Basic editor functions Copy, Paste, Cut, Undo, Redo and so on
4. Save and Save as files
5. Close files
6. Run commands using the command palette
7. Use basic keyboard shortcuts
8. Be able to change settings from the app
9. Have a clean, usable, and responsive UI

---

## **Post-MVP – Extensions & Customization Roadmap**

### **Phase 6 – Extensions Support**

* [ ] Basic Extensions System
  * [ ] Load extensions from a local folder
  * [ ] Enable/disable extensions
  * [ ] Automatic detection of new extensions
* [ ] Extension structure guidelines
  * [ ] Define manifest file (name, version, supported languages, commands)
  * [ ] Define how themes / grammars / commands are packaged
* [ ] Simple API hooks for extensions
  * [ ] Add syntax highlighting
  * [ ] Add editor commands
  * [ ] Add menu items or toolbar buttons

---

### **Phase 7 – Extension API**

* [ ] Expose editor APIs to extensions
  * [ ] Access current document content
  * [ ] Modify document content
  * [ ] Listen to editor events (file opened, saved, text changed)
* [ ] Command API
  * [ ] Register new commands for the command palette
  * [ ] Register shortcuts for commands
* [ ] UI API
  * [ ] Add side panels, status bar items, or overlays
  * [ ] Add dialogs or notifications

---

### **Phase 8 – Other Customization Options**

* [ ] Advanced theming
  * [ ] Load TextMate-compatible themes dynamically
  * [ ] Allow users to tweak editor colors, fonts, and cursor styles
* [ ] Keybindings customization
  * [ ] User-defined shortcuts
  * [ ] Shortcut profiles
* [ ] Workspace settings
  * [ ] Per-folder/project settings (override global settings)
* [ ] File-specific overrides
  * [ ] Language-specific settings (tab size, auto-indent rules, etc.)
* [ ] Toolbar with common actions
* [ ] Accessibility considerations (keyboard navigation, font scaling)
* [ ] Goto line/col

---

### **Phase 9 – Polishing Extension Ecosystem**

* [ ] Extension marketplace (optional MVP)
  * [ ] Local listing of installed extensions
  * [ ] Enable/disable/update extensions
* [ ] Extension safety
  * [ ] Simple sandboxing
  * [ ] Error handling (prevent crashing the editor)
* [ ] Documentation & Samples
  * [ ] Provide sample extension templates
  * [ ] Document all API methods and hooks

---

## Beta Roadmap

LSP

Syntax highlighting (Treesitter, LSP)

## Release Roadmap

Polishing UI/UX

API Polish

## Hotfixes

## Updates
