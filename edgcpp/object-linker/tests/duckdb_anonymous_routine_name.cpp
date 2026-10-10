struct NamedValue {
    int value;
    NamedValue(int initial = 3) noexcept : value(initial) {}
};
struct NamedOwner {
    struct {
        NamedValue first;
        NamedValue second{5};
    } helpers;
};
int main() {
    NamedOwner owner;
    return owner.helpers.first.value == 3 && owner.helpers.second.value == 5 ? 0 : 1;
}
