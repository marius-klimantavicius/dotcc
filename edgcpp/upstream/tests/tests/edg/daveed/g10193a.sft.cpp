//remark:Bit field diagnostics
//options:-DPOS;rp:-DNEG;fn

enum E {
    zero, one, two, big=0x1111
};

template <typename T, int n>
struct S {
    T t36 : 36;     /* should not warn */
    enum E e : n;   /* should not warn */
};

#ifdef NEG
struct S2 {
    double f : 36;  /* assume 36 <= sizeof(double);
                       then the warning makes no sense */
};
#endif

int main() {
    S<unsigned long long, 16> s;
    s.t36 = 0;
    return s.t36;
}
