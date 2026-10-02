//remark:alias-declaration as init-stmt
//options:--c++23;fn

namespace N { using X = int;};
int main() {
    if (using N::X; int n = 1);
}
