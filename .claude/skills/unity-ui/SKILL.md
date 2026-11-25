---
name: unity-ui
description: Build and optimize Unity UI with UI Toolkit and UGUI, responsive layouts, event systems, and performance optimization.
requires:
  - unity-csharp-fundamentals
---

# Unity UI - User Interface Systems

## Overview
Unity UI systems covering both UI Toolkit (modern) and UGUI (Canvas-based legacy).

**Foundation Required**: `unity-csharp-fundamentals` (TryGetComponent, FindAnyObjectByType, null-safe coding)

**Core Topics**:
- UI Toolkit (UIElements) for editor and runtime
- UGUI (Canvas) for game UI
- Responsive layouts and anchors
- Event systems
- UI performance optimization
- Data binding patterns

## Quick Start

### UGUI Button
```csharp
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button mActionButton;
    [SerializeField] private Text mStatusText;

    void Start()
    {
        mActionButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        mStatusText.text = "Button clicked!";
    }
}
```

### UI Toolkit (Runtime)
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    void OnEnable()
    {
        UIDocument uiDocument;
        if (TryGetComponent(out uiDocument))
        {
            VisualElement root = uiDocument.rootVisualElement;
            Button button = root.Q<Button>("action-button");
            button.clicked += OnButtonClick;
        }
    }

    void OnButtonClick()
    {
        Debug.Log("Button clicked!");
    }
}
```

## UI Systems Comparison

| Feature | UI Toolkit | UGUI |
|---------|-----------|------|
| Performance | Better in many UI-heavy cases | Good |
| Styling | USS (CSS-like) | Inspector |
| Layout | Flexbox | RectTransform |
| Best for | Complex tools and responsive UI | Established game UI and Canvas workflows |
| Learning curve | Steeper | Easier |

## Canvas Optimization (UGUI)
```csharp
// Separate static and dynamic content into different canvases.
// Disable raycast targets on non-interactive graphics.
[SerializeField] private Image mBackground;

void Start()
{
    mBackground.raycastTarget = false;
}

// Use CanvasGroup for fades without rebuilding the Canvas.
CanvasGroup canvasGroup;
if (panel.TryGetComponent(out canvasGroup))
{
    canvasGroup.alpha = 0.5f;
}
```

## Responsive Layout
```csharp
CanvasScaler scaler;
if (TryGetComponent(out scaler))
{
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920, 1080);
    scaler.matchWidthOrHeight = 0.5f;
}
```

## Best Practices
1. Separate static and dynamic canvases where that reduces rebuild cost.
2. Disable raycasts on non-interactive graphics.
3. Use CanvasGroup for fades and transitions.
4. Atlas UI textures where appropriate.
5. Pool frequently opened/closed UI elements rather than repeatedly destroying them.
6. Test multiple resolutions and aspect ratios.
7. Profile UI rebuilds and event handling.
8. Keep UI state and gameplay state decoupled.
9. Preserve existing button actions and navigation contracts when redesigning a screen.

## Codebound safeguards
- Inspect the existing UI implementation before choosing UI Toolkit or UGUI.
- Do not migrate the entire project UI during a single-screen redesign.
- Keep the main menu independent from gameplay HUD elements.
- Do not fake unavailable features with decorative buttons.
- Verify focus, keyboard/controller navigation, scaling, and actual button behavior.
