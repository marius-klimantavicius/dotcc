# RoslynObj C# translator — proposed project

Status: design for review, 2026-10-15. No translator implementation exists here yet.

This separate consumer will translate a set of `.roslynobj` translation units
into readable unsafe C#. EDG remains the authority for C/C++ semantics; the
translator consumes the public `RoslynObj` model and implements its recorded
execution in C#. The reader remains independent of the translator.

The first runnable real target is the existing full-feature SQLite amalgamation.
Unsafe code is the normal output: preserve pointers, explicit storage, names and
ordinary source-like operations wherever C# can express the recorded behavior.

- [Design and implementation plan](docs/PLAN.md)
- [Acceptance milestones and SQLite requirements](docs/ACCEPTANCE.md)

Implementation starts after approval of this plan. Existing object exports and
reader acceptance are input evidence, not evidence that translation works.
