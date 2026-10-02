//type:fp
//options_all:--c++17 -tused -A
struct S {
~S() noexcept(false) = default; // OK, despite mismatched exception specification
};
