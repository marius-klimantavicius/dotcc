int constructed;
struct Value { int n; Value() : n(42) { ++constructed; } };
Value global;
int main() { return global.n == 42 && constructed == 1 ? 0 : 1; }
