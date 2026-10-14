struct OverrideRoot { virtual int call() { return 1; } };
struct OverrideMiddle : OverrideRoot { int call() override { return 2; } };
struct OverrideLeaf : OverrideMiddle {};
OverrideLeaf override_leaf;
int observe_override(OverrideLeaf& object) { return object.call(); }

struct PrimaryRoot { virtual int call() { return 3; } };
struct PrimaryMiddle : PrimaryRoot {};
struct PrimaryLeaf : PrimaryMiddle {};
PrimaryLeaf primary_leaf;

struct AttributedBase {};
struct AttributedDerived : [[roslyn_probe::marker]] AttributedBase {};
AttributedDerived attributed_derived;
