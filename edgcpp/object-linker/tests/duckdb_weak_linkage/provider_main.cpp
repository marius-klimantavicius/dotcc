extern "C" __attribute__((weak)) int optional();
extern "C" int activate();
int (*optional_pointer)() = optional;
int main() {
#ifdef ACTIVATE
    if (activate()!=5 || !optional || !optional_pointer) return 3;
    return optional()!=31 || optional_pointer()!=31;
#else
    return optional!=0 || optional_pointer!=0;
#endif
}
