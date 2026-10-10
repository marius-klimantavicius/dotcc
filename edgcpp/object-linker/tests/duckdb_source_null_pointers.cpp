template<class T> concept NullPointers = requires(T value) {
    (int*)0;
    (const long*)0;
    (int (*)(int))0;
    value + 7;
};
template<NullPointers T> struct Holder { T value = {}; };
static_assert(NullPointers<int>);
int main() { return 0; }
