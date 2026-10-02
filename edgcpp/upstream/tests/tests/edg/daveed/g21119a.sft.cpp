//remark:noexcept and core constant expressions
//options:--c++17;fp

int main(void) {
 static_assert(!noexcept((true ? 0 : throw 1)), "error");
 static_assert(!noexcept((false ? throw 0 : 1)), "error");
 return 0;
}
