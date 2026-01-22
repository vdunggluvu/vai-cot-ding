# MagicMouseClone — Test Checklist

**Version:** 1.0  
**Date:** _______________  
**Tester:** _______________  
**Environment:** Windows ___ / DPI ___% / Monitors ___

---

## Pre-Test Setup

- [ ] .NET 8.0 SDK installed (`dotnet --version` shows 8.0.x)
- [ ] Project extracted to test folder
- [ ] Build succeeded: `.\build.ps1` → ✅ Build completed successfully
- [ ] No antivirus blocking executable

---

## CRITICAL Tests (Must Pass)

- [ ] **TC-01:** App starts without errors
- [ ] **TC-01:** Tray icon appears in system tray
- [ ] **TC-01:** Balloon tip: "Application started"
- [ ] **TC-02:** Second instance shows "Already running" message
- [ ] **TC-02:** Only 1 process in Task Manager
- [ ] **TC-08:** Exit from tray → app closes completely
- [ ] **TC-08:** Config files preserved after exit

---

## HIGH Priority Tests

- [ ] **TC-03:** Right-click tray → context menu appears
- [ ] **TC-03:** "Show Window" menu item works
- [ ] **TC-03:** Double-click tray icon shows window
- [ ] **TC-04:** Main window displays correctly
- [ ] **TC-04:** "Status: Running" label visible
- [ ] **TC-04:** Device label shows "Connected"
- [ ] **TC-04:** Minimize to tray (not taskbar)
- [ ] **TC-04:** Close button ([X]) hides to tray (doesn't exit)
- [ ] **TC-05:** Gestures appear in log within 30 seconds
- [ ] **TC-05:** Multiple gesture types detected
- [ ] **TC-07:** Config folder created in %APPDATA%
- [ ] **TC-07:** config.json exists and is valid JSON
- [ ] **TC-07:** Profiles\Default.json exists
- [ ] **TC-10:** CPU usage < 2% when idle
- [ ] **TC-10:** Memory usage < 50 MB

---

## MEDIUM Priority Tests

- [ ] **TC-06:** "Device Status" shows correct info
- [ ] **TC-06:** Battery level displayed (85%)
- [ ] **TC-09:** App recovers from invalid config.json
- [ ] **TC-11:** CPU < 5% during gesture generation
- [ ] **TC-11:** UI remains responsive
- [ ] **TC-13:** UI readable on high DPI (150%)
- [ ] **TC-14:** App stable after 1 hour runtime

---

## LOW Priority Tests

- [ ] **TC-12:** Window moves correctly to secondary monitor
- [ ] **TC-12:** Tray icon on correct taskbar

---

## Edge Cases (Attempt At Least 3)

- [ ] **Edge-01:** Corrupt profile file → app recreates defaults
- [ ] **Edge-02:** No write permissions → app continues
- [ ] **Edge-03:** Rapid start/stop → no zombie process
- [ ] **Edge-04:** System shutdown → clean exit
- [ ] **Edge-05:** Invalid profile → error logged, continues

---

## Performance Metrics

| Metric | Target | Actual | Pass/Fail |
|--------|--------|--------|-----------|
| CPU (Idle) | < 2% | ___% |  |
| CPU (Active) | < 5% | ___% |  |
| Memory (Idle) | < 50 MB | ___ MB |  |
| Memory (Active) | < 60 MB | ___ MB |  |
| Startup Time | < 3s | ___s |  |

---

## Bugs Found

| ID | Severity | Description | TC |
|----|----------|-------------|-----|
| 1 |  |  |  |
| 2 |  |  |  |
| 3 |  |  |  |

**Severity Levels:** CRITICAL / HIGH / MEDIUM / LOW

---

## Final Assessment

**Total Tests Run:** ___ / 34  
**Passed:** ___  
**Failed:** ___  
**Skipped:** ___  
**Blocked:** ___  

**Pass Rate:** ___%

**Recommendation:**
- [ ] ✅ Approve for release
- [ ] ⚠️ Approve with known issues
- [ ] ❌ Require fixes before release
- [ ] 🔄 Needs additional testing

---

## Notes

```
[Additional observations, performance notes, UX feedback]









```

---

**Tester Signature:** _______________  
**Date Completed:** _______________

