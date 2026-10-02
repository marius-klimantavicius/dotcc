// Pointer operations on individual scalar objects; one-past is never dereferenced.
int value = 17;
int *forward = &value;
int **indirect = &forward;
int *empty = nullptr;
int *end = &value + 1;
alignas(64) long aligned = 29;
unsigned long long maximum = 0xffffffffffffffffULL;

void add(int *p, int n) { *p += n; }
int read(const int *p) { return *p; }
int &identity(int &r) { return r; }
int parameter(int n) { int *p = &n; add(p, 4); return n; }
int trace = 0;
int sink = 0;
int *lhs() { trace = trace * 10 + 2; return &sink; }
int rhs() { trace = trace * 10 + 1; return 3; }
int index() { trace = trace * 10 + 1; return 0; }
int main() {
    if (empty || forward == nullptr || *indirect != &value) return 1;
    if (end - forward != 1 || end - 1 != forward) return 2;
    int local = 5;
    int *p = &local;
    int **pp = &p;
    add(*pp, 3);
    if (local != 8 || read(p) != 8) return 3;
    *p = 12;
    ++*p;
    if (local != 13) return 4;
    int *next = p++;
    if (p - next != 1) return 5;
    --p;
    p += 1;
    p -= 1;
    if (p != &local || p[0] != local || 0[p] != local) return 6;
    if (parameter(7) != 11) return 7;
    identity(local) += 2;
    if (local != 15) return 8;
    void *erased = p;
    int *restored = static_cast<int*>(erased);
    if (restored != p || *restored != 15) return 9;
    unsigned long bits = reinterpret_cast<unsigned long>(p);
    if (reinterpret_cast<int*>(bits) != p) return 10;
    if (reinterpret_cast<unsigned long>(&aligned) % 64 != 0) return 11;
    if (reinterpret_cast<unsigned long>(&local) % alignof(int) != 0) return 12;
    if (*(&maximum) != 0xffffffffffffffffULL) return 13;
    *lhs() = rhs();
    if (trace != 12 || sink != 3) return 14;
    trace = 0;
    *lhs() += rhs();
    if (trace != 12 || sink != 6) return 15;
    trace = 0;
    int selected = index()[lhs()];
    if (trace != 12 || selected != 6) return 16;
    return 0;
}
