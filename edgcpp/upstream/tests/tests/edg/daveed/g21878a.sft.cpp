//remark:inline template static data members
//options:--c++17;rp

template <class> struct A{};

struct S00 {
    template <class T> static inline T m = 100;
};

template <class> class C01 {
public:
    template <int I> static inline int m = I;
};

namespace N {
    struct S02 {
        template <class T> static inline T m = 102;
    };

    template <template <class> class> class C03 {
    public:
        template <int I> static inline int m = I;
    };
}


int main(void)
{
        int g;

g = S00::m<int>;
        g = C01<int>::m<101>;
        g = N::S02::m<int>;
        g = N::C03<A>::m<103>;

}
