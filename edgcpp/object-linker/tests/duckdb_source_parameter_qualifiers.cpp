template<class T> concept Values = requires(const T value, volatile T other, const volatile T both) {
    value + other; both + value;
};
template<Values T> struct Holder { T value = {}; };
int main() { return 0; }
