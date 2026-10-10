extern "C" void free(void *);
void operator delete(void *address, unsigned long) noexcept { free(address); }

struct SharedState {
    int value;
    explicit operator bool() const { return value != 0; }
    explicit SharedState(int input) : value(input) {}
};
struct LeftView : virtual SharedState {
    long long left;
    explicit LeftView(int input) : SharedState(input), left(17) {}
};
struct OtherStorage {
    long long first, second, third;
    OtherStorage() : first(21), second(22), third(23) {}
};
struct LargerObject : OtherStorage, LeftView {
    long long last;
    explicit LargerObject(int input) : SharedState(input), OtherStorage(), LeftView(input), last(29) {}
};
static int evaluations;
LeftView &evaluate(LeftView *object) { ++evaluations; return *object; }
SharedState *select_state(LeftView *object) { return &evaluate(object); }
int select_value(LeftView *object) { return evaluate(object).value; }
int select_truth(LeftView *object) { if (evaluate(object)) return 1; return 0; }
int main() {
    LeftView first(31);
    LargerObject second(47);
    SharedState *a = select_state(&first);
    SharedState *b = select_state(&second);
    if (evaluations != 2 || a->value != 31 || b->value != 47) return 1;
    if (select_value(&first) != 31 || select_value(&second) != 47 || evaluations != 4) return 2;
    if (!select_truth(&first) || !select_truth(&second) || evaluations != 6) return 4;
    a->value = 53; b->value = 59;
    return first.value == 53 && second.value == 59 && second.first == 21 && second.last == 29 ? 0 : 3;
}
