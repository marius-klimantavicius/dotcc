enum class Mode : unsigned short { Ready = 13 };
struct Defaults {
    int value = 7;
    const int *pointer = nullptr;
    bool enabled = true;
    long long count = -9;
    double fraction = 1.25;
    Mode mode = Mode::Ready;
    Defaults() {}
    Defaults(int chosen) : value(chosen) {}
};
struct Inherited : Defaults {
    int own = 5;
    Inherited() : Defaults(17) {}
};
template<int N> struct Number { int value = N; Number() {} };
int provider_defaults();
