//remark:Generated move operations
//options:--c++11;rp

int flag;

struct A {
        A() {}
        A(const A&) {}
};

struct B {
        B() {}
        B(B&&) {flag = 1;}
};

struct C : B {
        A x;
        C(int) {}
};

int main()
{
        C c(37);
        C d = (C&&)c;
        if (!flag)
                return 1;

        return 0;
}
