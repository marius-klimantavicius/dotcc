#if __STDC_VERSION__ >= 202311L
static_assert(sizeof(typeof(17)) == sizeof(int));
static int classify(typeof(17) value) { return value == 17; }
int main(void) { return !classify(17) || nullptr != (void*)0; }
#else
_Static_assert(sizeof(long)==8,"portable LP64 contract");
int main(void) { _Generic(1,int: (void)0); return 0; }
#endif
