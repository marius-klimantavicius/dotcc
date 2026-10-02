//type: fp
//options: 
# 0 "./Wstringop-overflow-28.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./Wstringop-overflow-28.c"




# 1 "./range.h" 1
# 11 "./range.h"
typedef int int32_t;
typedef long int ptrdiff_t;
typedef long unsigned int size_t;

static inline ptrdiff_t signed_value (void)
{
  extern volatile ptrdiff_t signed_value_source;
  return signed_value_source;
}

static inline size_t unsigned_value (void)
{
  extern volatile size_t unsigned_value_source;
  return unsigned_value_source;
}

static inline ptrdiff_t signed_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return val < min || max < val ? min : val;
}

static inline ptrdiff_t signed_anti_range (ptrdiff_t min, ptrdiff_t max)
{
  ptrdiff_t val = signed_value ();
  return min <= val && val <= max ? min == (-0x7fffffffffffffffL - 1) ? max + 1 : min - 1 : val;
}

static inline size_t unsigned_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return val < min || max < val ? min : val;
}

static inline size_t unsigned_anti_range (size_t min, size_t max)
{
  size_t val = unsigned_value ();
  return min <= val && val <= max ? min == 0 ? max + 1 : min - 1 : val;
}
# 6 "./Wstringop-overflow-28.c" 2







extern void* alloca (size_t);
extern void* calloc (size_t, size_t);
extern void* malloc (size_t);

extern __attribute__ ((alloc_size (1), malloc)) char* alloc1 (size_t);
extern __attribute__ ((alloc_size (1, 2), malloc)) char* alloc2 (size_t, size_t);

extern char* strcpy (char*, const char*);

void sink (void*, ...);





void same_size_and_offset_idx_cst (void)
{
# 39 "./Wstringop-overflow-28.c"
  {
    const size_t n = unsigned_range ((2), (3));

    do { size_t n_ = n; ptrdiff_t i_ = -4; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);

    do { size_t n_ = n; ptrdiff_t i_ = -3; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -2; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 0; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);

  }

  {
    const size_t n = unsigned_range ((3), (4));

    do { size_t n_ = n; ptrdiff_t i_ = -5; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);

    do { size_t n_ = n; ptrdiff_t i_ = -4; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -3; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -2; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 0; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);

  }

  {
    const size_t n = unsigned_range ((5), (0xffffffffffffffffUL - 2));
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += n; p_[i_] = 0; sink (p_); } while (0);
  }
}






void different_size_and_offset_idx_cst (void)
{
  {
    const size_t n = unsigned_range ((2), (3));
    const size_t i = unsigned_range ((1), (2));

    do { size_t n_ = n; ptrdiff_t i_ = -4; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);

    do { size_t n_ = n; ptrdiff_t i_ = -3; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);

    do { size_t n_ = n; ptrdiff_t i_ = -2; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 0; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 1; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 2; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);

  }

  {
    const size_t n = unsigned_range ((3), (4));
    const size_t i = unsigned_range ((2), (5));

    do { size_t n_ = n; ptrdiff_t i_ = -6; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);





    do { size_t n_ = n; ptrdiff_t i_ = -5; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -4; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -3; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -2; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = -1; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 0; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 1; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = 2; char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);

  }
}





void different_size_and_offset_idx_var (void)
{
  {
    const size_t n = unsigned_range ((3), (4));
    const size_t i = unsigned_range ((1), (2));

    do { size_t n_ = n; ptrdiff_t i_ = signed_range (((-0x7fffffffffffffffL - 1)), (0)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = signed_range ((-3), (0)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = signed_range ((-1), (0)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = signed_range ((0), (1)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = signed_range ((1), (2)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = signed_range ((2), (3)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);
    do { size_t n_ = n; ptrdiff_t i_ = signed_range ((3), (4)); char *p_ = alloc1 (n_); p_ += i; p_[i_] = 0; sink (p_); } while (0);

 }
}


void ptr_add_2 (int n, int i0, int i1)
{
  if (n < 1 || 2 < n) n = 2;

  if (i0 < 0 || 1 < i0) i0 = 0;
  if (i1 < 1 || 2 < i1) i1 = 1;

  char *p = (char*)__builtin_malloc (n);
  char *q = p;

  q += i0;
  q[0] = 0;
  q += i1;
  q[0] = 1;
  q[1] = 2;

  sink (p, q);
}

void ptr_add_3 (int n, int i0, int i1, int i2)
{
  if (n < 3 || 4 < n) n = 3;

  if (i0 < 0 || 1 < i0) i0 = 0;
  if (i1 < 1 || 2 < i1) i1 = 1;
  if (i2 < 2 || 3 < i2) i2 = 2;

  char *p = (char*)__builtin_malloc (n);
  char *q = p;

  q += i0;
  q[0] = 0;
  q += i1;
  q[0] = 1;
  q[1] = 2;
  q += i2;
  q[0] = 3;
  q[1] = 4;

  sink (p, q);
}

void ptr_add_4 (int n, int i0, int i1, int i2, int i3)
{
  if (n < 7 || 8 < n) n = 7;

  if (i0 < 0 || 1 < i0) i0 = 0;
  if (i1 < 1 || 2 < i1) i1 = 1;
  if (i2 < 2 || 3 < i2) i2 = 2;
  if (i3 < 3 || 4 < i3) i3 = 3;

  char *p = (char*)__builtin_malloc (n);
  char *q = p;

  q += i0;
  q[0] = 0;
  q += i1;
  q[0] = 1;
  q[1] = 2;
  q += i2;
  q[0] = 3;
  q[1] = 4;
  q[2] = 5;
  q += i3;
  q[0] = 6;
  q[1] = 7;
  q[2] = 8;

  sink (p, q);
}

void ptr_sub_from_end (int n, int i0, int i1, int i2, int i3)
{
  if (n < 1 || 2 < n) n = 2;

  char *p = (char*)__builtin_malloc (n);
  char *q = p;


  q += n;
  q[-1] = 0;
  q[-2] = 1;
  q[-3] = 2;




  q[0] = 2;
  q[1] = 3;

  sink (p, q);
}
