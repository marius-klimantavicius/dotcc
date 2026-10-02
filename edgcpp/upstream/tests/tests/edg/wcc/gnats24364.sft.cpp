//type:fp
//options:--g++ --gnu_version 70100:--g++ --gnu_version 80100:--clang_version=30900 --c++11:--clang_version=40900 --c++11:--c++17:--c++11;fn:--c++14;fn
//options_all:-W

namespace {

[[maybe_unused]] const int z = 1;

void foo() {
  [[maybe_unused]] const int y = 1;
}

}  // namespace

int main() {
  foo();
  return 0;
}
