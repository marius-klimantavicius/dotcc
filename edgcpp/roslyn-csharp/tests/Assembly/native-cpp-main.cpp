#include <cstdio>
extern int assembly_cpp_entry(void);
extern int assembly_cpp_second(void);
int main()
{
    int first = assembly_cpp_entry();
    int second = assembly_cpp_second();
    std::printf("%d %d\n", first, second);
    return first != 75 || second != 8;
}
