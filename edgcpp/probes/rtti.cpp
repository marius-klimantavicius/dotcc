struct Base { virtual ~Base() {} };
struct Derived : Base { int n = 42; };
int main() {
    Derived d;
    Base* b = &d;
    Derived* p = dynamic_cast<Derived*>(b);
    return p && p->n == 42 ? 0 : 1;
}
