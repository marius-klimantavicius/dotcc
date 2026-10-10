extern "C" int selected();
extern "C" int from_fallback();
extern "C" __attribute__((weak)) int absent();
int (*absent_pointer)() = absent;
int (*selected_pointer)() = selected;
#ifndef EXPECTED
#define EXPECTED 11
#endif
int main() {
    if (absent || absent_pointer) return 1;
    if (selected()!=EXPECTED || selected_pointer()!=EXPECTED || from_fallback()!=EXPECTED) return 2;
    return 0;
}
