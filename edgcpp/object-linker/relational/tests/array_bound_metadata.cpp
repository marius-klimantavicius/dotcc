template<int N> struct Sized { int values[N + 1]; };
template<class T> struct MemberBound { int values[T::extent]; };
template<class... T> struct PackBound { int values[sizeof...(T) + 1]; };
struct Bounds { enum { extent = 2 }; };

extern void browse_sized(Sized<3>*);
extern void browse_member(MemberBound<Bounds>*);
extern void browse_pack(PackBound<int, float>*);
extern void browse_fixed(int (*)[3]);
extern void browse_zero(int (*)[0]);
extern void browse_incomplete(int (*)[]);

int main() {
    int fixed[3] = {1, 2, 3};
    return fixed[2] - 3;
}
