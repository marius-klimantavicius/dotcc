/* Weakref permits an unresolved target in EDG IL, but C# must not invent it. */
static int unresolved_view __attribute__((weakref("missing_alias_storage")));
int main(void) { return unresolved_view; }
