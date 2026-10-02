//type:rp
//options_all:--c++17 -tused -A
extern "C" int printf(const char*,...);
template<typename T> using id = T;
struct S {
   S() { printf("constructed\n"); }
  ~S() { printf("destroyed\n"); }
};
S s;
int i = 1;
bool cond = false;
int main()
{
    printf("pre static_cast\n");
    const S& b = static_cast<const S&>(s); // temporary int has same lifetime as b
    printf("post static_cast\n");
}
