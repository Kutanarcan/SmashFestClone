# CLAUDE.md — Unity / C# Working Rules

## 0. Communication
- Keep explanations short, precise, bulleted. No intro sentence, no closing summary.
- Do not dump depth unprompted. If there is more, leave one line: "I can expand on X if you want."
- Never guess when unsure — ask.
- Match the user's language; keep technical terms in English.

## 1. Architecture: C# Core, Thin Unity Shell
Game logic lives in a pure C# layer that does not reference `UnityEngine`. This is not a convention — it is enforced by assembly definitions:

```
Scripts/
  Core/       (Game.Core.asmdef)          -> NO UnityEngine reference
  Runtime/    (Game.Runtime.asmdef)       -> Core + UnityEngine
  Tests/
    EditMode/ (Game.Tests.EditMode.asmdef)-> Core
    PlayMode/ (Game.Tests.PlayMode.asmdef)-> Runtime
```

Because Core does not reference UnityEngine, accidentally calling a Unity API is a **compile error**. Testability is guaranteed by the compiler, not by discipline.

## 2. MonoBehaviour = Dumb Adapter
A MonoBehaviour may only:
- Carry serialized data from the Editor (`[SerializeField]`)
- Forward lifecycle events into Core
- Call Unity APIs (Instantiate, Transform, Animator, Audio…)

Forbidden: business-rule `if`s, calculations, state machines, caching logic, data transformation.

- Never do work inside `Update()` — call `ITickable.Tick(float dt)`.
- No singletons. Use one composition root (`GameInstaller : MonoBehaviour`) with manual wiring.
- MonoBehaviours have no constructors — inject dependencies via `Initialize(...)`.

## 3. Testability
- Check for every new type: "Can this be `new`ed without opening Unity?" If no, the design is wrong.
- Put everything non-deterministic behind an interface: `ITimeProvider`, `IRandomSource`, `IInputSource`, `IClock`.
- EditMode tests by default. PlayMode tests only when the Unity runtime is genuinely required.
- Prefer hand-written fakes over mocks — the fake is also documentation.
- Every phase ends by naming which test turns green.

## 4. Size
- If a class exceeds 150 lines, split it and state why.
- If a method exceeds 30 lines or 3 levels of nesting, extract.
- One public type per file.

## 5. SOLID — Practical Checks
- **SRP:** If describing the class requires "and", it is two classes.
- **OCP:** A `switch (enumType)` is a polymorphism candidate — but only if it will actually grow.
- **LSP:** Overriding to throw `NotImplementedException` means the hierarchy is wrong.
- **ISP:** An interface with 5+ members is suspect.
- **DIP:** Core never looks at Runtime (asmdef already prevents it).

## 6. Design Pattern Usage
- Simplest working solution first. Propose a pattern only when a concrete problem exists: an axis of change, a testing barrier, the third repetition.
- When proposing a pattern, state in one sentence **what it solves**. If you cannot, do not propose it.
- Banned reflexes: singleton for everything, factory with a single implementation, unnecessary observer layers, premature abstract factory.

## 7. Phase-Based Progress
- One **actionable phase** per answer. Never write the whole architecture at once.
- Phase format:
  - **Goal:** one sentence
  - **Files touched:** list
  - **Done when:** which test turns green
- List later phases as headlines only — no code for them.
- Explaining the conceptual big picture is fine; scattering its implementation is not.

## 8. Garbage / Allocation
Separate the two modes and say which one you are in:

**Test / prototype mode:** allocation allowed, speed first. Always append a note: "In production this allocation is removed by …".

**Production mode — forbidden on hot paths:**
- LINQ, closure capture, string concatenation, `params`, boxing (including `foreach` over an interface enumerator)
- Per-frame `new` (especially `List`, arrays, delegates)

**Non-alloc alternatives:**
- `Physics.RaycastNonAlloc` / `RaycastCommand`
- Reused buffer `List<T>` + `Clear()`
- Object pooling (VFX, projectiles, UI items)
- `struct` + `in` parameters, `Span<T>` / `stackalloc`
- Justify `class` vs `struct` by lifetime and copy cost.

## 9. Performance Claims
- No performance claim without evidence. Accepted evidence: Profiler capture, `Stopwatch` measurement, `GC.GetTotalMemory` delta, IL/alloc analysis.
- When saying "faster", state **how much, for which N, on which platform**.
- Never optimize without measuring. If uncertain, offer a micro-benchmark and provide its skeleton.
- Benchmark skeleton: Unity Performance Testing package (`[Test, Performance]`, `Measure.Method().WarmupCount(5).MeasurementCount(20)`) or EditMode `Stopwatch` + allocation measurement.
- Every benchmark includes: warmup, fixed N, allocation measurement, comparison table.
- For mobile targets, separately warn about IL2CPP and managed stripping behavior.

## 10. Algorithm Explanation
Before writing code, 3–6 lines:
- What it does
- Why this approach
- Time / memory complexity
- Assumptions and edge cases

If alternatives were considered, one line on why each was rejected.

## 11. Answer Format
1. Algorithm / approach note (short)
2. Code
3. Tests (if any)
4. Warnings: performance, allocation, edge cases — only if real
5. Next phase: one headline
