# RaStudio.py Converters (Phase 3, Python)

This folder contains **Python value converters** for RaStudio.py UI.
Converters are used for data binding in PyQt, Kivy, Textual, etc., to transform values for display, visibility, and theming—enabling a modular, plugin-ready, and visually rich interface.

## Converters Overview

- **CommandsVisibilityConverter**  
  Controls visibility of Commands panels/cards based on category or plugin extension.

- **LogsVisibilityConverter**  
  Controls visibility of Logs panels/cards based on category or plugin extension.

- **MetricsVisibilityConverter**  
  Controls visibility of Metrics panels/cards based on category or plugin extension.

- **StatusVisibilityConverter**  
  Controls visibility of Status panels/cards based on category or plugin extension.

- **MessageTypeToBrushConverter**  
  Maps log/message types (e.g., log, error, info, debug, etc.) to color strings for theming and diagnostics display. Supports plugin-registered custom types/colors.

## Usage

In Python UI code:

```python
from core.converters.message_type_to_brush import MessageTypeToBrushConverter
from core.converters.commands_visibility_converter import CommandsVisibilityConverter

color = MessageTypeToBrushConverter().convert(viewmodel.message_type)
visible = CommandsVisibilityConverter().convert(viewmodel.panel_category)
```

## Extending Converters

- **Plugins and extensions** can register custom message types and colors to `MessageTypeToBrushConverter`.
- Visibility converters support flexible category targeting via their constructor.

---