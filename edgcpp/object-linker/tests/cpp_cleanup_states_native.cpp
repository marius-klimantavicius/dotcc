extern "C" void cleanup_reset();
extern "C" unsigned long long cleanup_trace();
extern "C" void cleanup_locals();
extern "C" void cleanup_constructor();
extern "C" void cleanup_members();
extern "C" void cleanup_temporary_result();
extern "C" void cleanup_delegating();
static int selected;
extern "C" void cleanup_throw_point(int point) { if (point == selected) throw point; }
static int check(void (*run)(), int point, unsigned long long expected) {
    selected = point; cleanup_reset(); bool caught = false;
    try { run(); } catch (int actual) { if (actual != point) return 1; caught = true; }
    return caught != (point != 0) || cleanup_trace() != expected;
}
int main() {
    return check(cleanup_locals, 0, 12721) || check(cleanup_locals, 1, 1) ||
           check(cleanup_locals, 2, 121) || check(cleanup_locals, 3, 1221) ||
           check(cleanup_constructor, 0, 3436875) || check(cleanup_constructor, 3, 3) ||
           check(cleanup_constructor, 4, 343) || check(cleanup_constructor, 6, 34365) ||
           check(cleanup_constructor, 8, 3436875) ||
           check(cleanup_members, 0, 4268725) || check(cleanup_members, 4, 4) ||
           check(cleanup_members, 2, 425) || check(cleanup_members, 6, 42625) ||
           check(cleanup_members, 8, 4268725) ||
           check(cleanup_temporary_result, 0, 333) || check(cleanup_temporary_result, 3, 3) ||
           check(cleanup_delegating, 0, 1283) || check(cleanup_delegating, 1, 1) ||
           check(cleanup_delegating, 2, 123) || check(cleanup_delegating, 8, 1283);
}
