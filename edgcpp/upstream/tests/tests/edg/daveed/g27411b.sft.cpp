//remark:C++20 generated comparisons
//options:--c++20;rp

struct A {
        int x;
        A(int i) : x(i) {}
        A& operator<=>(int i) {x = i; return *this;}
        operator int() {return x;}
};

int main()
{
        A a(29);

        bool b1 = a < 37;
        bool b2 = a <= 37;
        bool b3 = a > 37;
        bool b4 = a >= 37;

        if (b1) return 1;
        if (b2) return 2;
        if (!b3) return 3;
        if (!b4) return 4;
        return 0;
}

