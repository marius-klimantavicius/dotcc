#ifdef NEG
#define WEAK
#else
#define WEAK __attribute__((weak))
#endif

void f() {}
short s;
int main() {
	f();
	extern void g();
	g();
}
