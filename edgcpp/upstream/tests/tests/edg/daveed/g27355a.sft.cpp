//remark:_Nullable conversions
//options:--c++14 --clang_v=150101;fp

int main() {
 int value = 0;
 int * _Nullable   p = &value;
 int * _Nullable * pp = &p;
 int *          *  qq = pp;
 int * _Nullable * rr = qq;
}
