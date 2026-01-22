# Test Checklist: Magic Mouse Utilities Clone

- [ ] **Build Verification**
  - [ ] `dotnet build` passes without errors.
  - [ ] `dotnet test` passes all units.

- [ ] **Launch & Tray**
  - [ ] App starts silently (no window, only tray).
  - [ ] Hovering tray icon shows correct tooltip.
  - [ ] Right-click menu works ("Settings", "Exit").
  - [ ] Double-click opens Settings.

- [ ] **Settings UI**
  - [ ] Window opens on request.
  - [ ] "Enable Gestures" checkbox toggles.
  - [ ] "Scroll Speed" slider moves.
  - [ ] Log box is read-only.
  - [ ] Closing window (X) minimizes to tray, DOES NOT Exit.

- [ ] **Functional Simulation**
  - [ ] Click "Connect Fake Device" works.
  - [ ] Logs show "Gesture Detected: ScrollDown" periodically.
  - [ ] Application does not crash during simulation.

- [ ] **Shutdown**
  - [ ] "Exit" from Tray terminates process completely.
