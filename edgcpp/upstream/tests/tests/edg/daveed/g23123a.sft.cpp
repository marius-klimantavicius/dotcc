//remark:Vector assignment
//options:--c11 --gnu=70300;fp

#if 0
typedef int vect_t __attribute__((__vector_size__(4))); /* ok */
#else
typedef long long vect_t __attribute__((__vector_size__(8))); /* fail */
#endif
vect_t a, b, c;
void foo (void) { a = (b == c); }
