//remark: Hiding of using-declarations
//options:-A;fp:;fp:--g++;fp:--microsoft;fp

struct B {
        template <class T> int f(T) { return 1; }
};

struct D : private B { 
        using B::f;
        template <class T> int f(T) { return 2; }
        void f(float, int) { return; };
};

void k(D* p)
{
        p->f(1);
}
