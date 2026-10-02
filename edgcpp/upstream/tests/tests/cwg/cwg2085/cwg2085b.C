// translation unit 2:
struct X {
X(int, int);
X(int, int, int);
};
X::X(int, int = 0, int = 0) { }
class D {
X x = 0;
};
D d2; // X(int, int, int) called by D();
// D()’s implicit definition violates the ODR
