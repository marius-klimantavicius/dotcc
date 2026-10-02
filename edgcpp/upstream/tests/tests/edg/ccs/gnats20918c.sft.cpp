//type:fp
//options::--g++:--clang:--microsoft
//options_all:--c++14
struct Inner {
    Inner() { }
    Inner(int, int) { }
};
struct Outer {
    Inner in;
    Outer() noexcept = default;
    Outer(int x, int y) : in(x, y) { }
};
int main() {
    Outer out(11, 22);
}
