// Source-only prototype widths must not inherit EDG's storage placeholders.
template<class T> struct Bits {
    using Size = typename T::size_type;
    Size flag : 1;
    Size capacity : sizeof(Size) * 8 - 1;
    Size read() const { return capacity; }
};
struct Traits { using size_type = unsigned long; };
template<unsigned N> struct Small {
    unsigned value : N;
    unsigned read() const { return value; }
};
template<class T> concept Inspect = requires(Bits<T> bits, Small<3> small) { bits.read(); small.read(); };
template<Inspect T> struct Retain { T value = {}; };
struct Wide { unsigned value : 40; unsigned : 0; };
int main() {
    Bits<Traits> bits = {1, 23};
    Small<3> small = {5};
    Wide wide = {9};
    return bits.read() != 23 || small.read() != 5 || wide.value != 9;
}
