extern "C" { __thread int raw_storage; }
extern int view __asm__("raw_storage");
int main() { return view; }
