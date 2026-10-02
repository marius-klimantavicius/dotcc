//type:fp
//options_all:--c++17
//remark:[6.1] Template rescan treating builtin binary operators as unary
// 4/17/20  [EDGcpfe/22272]
//
// Template rescan treating builtin binary operators as unary
//
// When constant folding is attempted on builtin operators during template
// instantiation, the front end was incorrectly treating certain binary operators
// as if they were unary operators.  This caused undesirable behavior including
// aborts.
//
// This is now fixed.
template <int a> struct b { static const int c = a; };
class f;
template <class d> struct g : b<__is_trivially_assignable(f, d)> {};
class f {
    template <typename e>
        void operator=(e) noexcept(b<__is_nothrow_assignable(int, int)>::c);
};
int var = g<f>::c;
