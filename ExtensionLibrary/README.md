# Code Editor – Extension API

The **Extension API** provides a simple and powerful way to extend the Code Editor with new functionality
from custom commands to UI components and integrations.

## Overview

Extensions allow developers to hook into the editor’s lifecycle and respond to events.  
They are loaded dynamically when the editor starts, or when installed at runtime.

Each extension should implement two main lifecycle methods:

``` csharp
void Activate() { /* setup logic */ }
void Deactivate() { /* cleanup */ }
```

## Available APIs

> The Extension API is still evolving.
> Future versions will include deeper access to the editor, file system, themes, and UI.

### Namespaces in development

- **CodeEditor.API.Commands** – Register and execute editor commands
- **CodeEditor.API.Editor** – Interact with open files, selections, and content
- **CodeEditor.API.Window** – Display in-editor messages
- **CodeEditor.API.UI** – Add custom panels, buttons, and widgets

### Planned namespaces

- **CodeEditor.API.Logger** – Add custom panels, buttons, and widgets

## Packaging & Distribution

**TBA** will come back when i have an answer

## License

Extensions are licensed under the same terms as the main project, unless stated otherwise.
MIT License © 2025 BjornBEs
