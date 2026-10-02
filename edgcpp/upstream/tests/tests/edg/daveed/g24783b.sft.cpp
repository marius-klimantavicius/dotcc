//remark:alias-declaration as init-stmt
//options:-DNEG --c++23;fn:--c++23;fp:--c++20;fp:--c++20 --diag_error=3466;fn

namespace N { using X = int;};
int main() {
    float x[3] = { 1, 2, 3 };
    for (using I = int; I p: x) {}
    if (using I = int; I n = 1);
#ifdef NEG
    if (using N::X; int n = 1);
#endif
}
