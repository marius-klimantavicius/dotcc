template<class T> constexpr T twice(T x) { return x + x; }
struct Value {
    int n;
    int add(int x) const { return n + x; }
};
int main() {
    Value v{20};
    auto add = [v](int x) { return v.add(x); };
    return twice(add(1)) == 42 ? 0 : 1;
}
