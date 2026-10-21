# Independent scalar-reference in-flight review, round 1

Reviewer `/root/manual_edg_supervisor/review_m3`, 2026-10-21. Source review only;
no implementation edits, builds/tests, native/compiler executions or Git work.
Integration is evolving and no execution/acceptance claim is made here.

Actual main-tree Types/Parser/Expressions integration was compared with the
held isolated patch. The newer distinct character types, target properties,
representability promotions and decoder branch are preserved. This must still
be confirmed by the full regression and new character/reference controls.

**REF-01 — Medium, source-confirmed resource exposure; no executed crash claim.**
`ReferenceBindingExpression.FindOrigin` recursively follows named-reference
initializers with a fresh visited set for every new binding. A valid chain of
references to the preceding reference has unbounded CLR recursion and repeated
quadratic origin traversal. The visited set prevents cycles but not a long
acyclic chain; neither the parser/interpreter depth limit nor a loop budget
guards this helper.

The source shape is ordinary supported scalar declarations: `int object=1;`
then `int& first=object; int& second=first; int& third=second;` continued. The
problem is the algorithm's growth with chain length, not a claim that these
three declarations crash. An iterative walk or reuse of prior immutable
ReferenceBinding origin/KnownTemporary facts can remove the stack growth while
preserving original temporary identity, owner and Unknown/self-cycle behavior.
A finite reference path must not acquire an unqualified claim of arbitrary
chain robustness from the existing short-chain tests. The supervisor was
notified promptly; final closure depends on actual produced code and execution.

No other critical/high defect has been identified in the reviewed core so far.
Scalar binding/category/ranking, original temporary ownership, recursive type
cache guards and optional/required constant boundaries still need the final
held integrated assertions/native witnesses. Full reference templates,
class/pointer/array/function reference semantics and lifetime/constexpr engines
remain required inventory gaps.

Source follow-up: main's FindOrigin now reuses an earlier completed binding's
immutable ReferentOrigin and KnownTemporary directly. It no longer recursively
walks alias initializers; a self/uncompleted initializer remains Unknown.
Authored512-link global/local source chains assert original temporary identity,
no replacement temporary and the local return warning. REF-01 is source-repaired
but still awaits matching managed/NativeAOT execution.

The integrated character fixture now covers char8 materialization payload/
macro origin, char16 reference-return/alias categories, char32 promotion versus
conversion/exact reference preference, wide direct binding, actual by-value
template replay/frozen lookup and failed/unsupported template scope collection.
No reference constexpr values are credited by those ordinary runtime graphs.

The test-only oracle adapter's optional Unicode source-kind option was also
read. Omitted and explicit none profiles remain distinct in argv/receipt state;
argument boundaries, used instantiation and exact status matching are preserved.
Two new profile controls are authored, unexecuted here. Tools belong in the
next held fingerprint map along with production/tests/fixtures.

A finite frozen-lookup witness was requested and accepted by the supervisor:
an early int-returning const-int-reference overload and later void-returning
mutable-reference overload make late lookup fail the template's return type.
This gives native used-instantiation acceptance a real discriminator beyond
fixed return-type decltype checks. It must be the same retained source used by
managed candidate/replay assertions; this is pending the worker's fresh hold.
