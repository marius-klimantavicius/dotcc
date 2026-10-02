//type: rp
//options: --c99 --strict_gnu
# 0 "./di-longlong64-sync-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./di-longlong64-sync-1.c"
# 20 "./di-longlong64-sync-1.c"
__extension__ typedef long unsigned int size_t;

extern void abort (void);
extern void *memcpy (void *, const void *, size_t);
extern int memcmp (const void *, const void *, size_t);


static long long AL[24];

static long long init_di[24] = { 0x100000002ll, 0x200000003ll, 0, 1,

     0x100000002ll, 0x100000002ll,
     0x100000002ll, 0x100000002ll,

     0, 0x1000e0de0000ll,
     42 , 0xc001c0de0000ll,

     -1ll, 0, 0xff00ff0000ll, -1ll,

     0, 0x1000e0de0000ll,
     42 , 0xc001c0de0000ll,

     -1ll, 0, 0xff00ff0000ll, -1ll};

static long long test_di[24] = { 0x1234567890ll, 0x1234567890ll, 1, 0,

     0x100000002ll, 0x100000002ll,
     0x100000002ll, 0x100000002ll,

     1, 0xc001c0de0000ll,
     20, 0x1000e0de0000ll,

     0x300000007ll , 0x500000009ll,
     0xf100ff0001ll, ~0xa00000007ll,

     1, 0xc001c0de0000ll,
     20, 0x1000e0de0000ll,

     0x300000007ll , 0x500000009ll,
     0xf100ff0001ll, ~0xa00000007ll };


static void
do_noret_di (void)
{
  __sync_val_compare_and_swap (AL+0, 0x100000002ll, 0x1234567890ll);
  __sync_bool_compare_and_swap (AL+1, 0x200000003ll, 0x1234567890ll);
  __sync_lock_test_and_set (AL+2, 1);
  __sync_lock_release (AL+3);



  __sync_val_compare_and_swap (AL+4, 0x000000002ll, 0x1234567890ll);
  __sync_val_compare_and_swap (AL+5, 0x100000000ll, 0x1234567890ll);
  __sync_bool_compare_and_swap (AL+6, 0x000000002ll, 0x1234567890ll);
  __sync_bool_compare_and_swap (AL+7, 0x100000000ll, 0x1234567890ll);

  __sync_fetch_and_add (AL+8, 1);
  __sync_fetch_and_add (AL+9, 0xb000e0000000ll);
  __sync_fetch_and_sub (AL+10, 22);
  __sync_fetch_and_sub (AL+11, 0xb000e0000000ll);

  __sync_fetch_and_and (AL+12, 0x300000007ll);
  __sync_fetch_and_or (AL+13, 0x500000009ll);
  __sync_fetch_and_xor (AL+14, 0xe00000001ll);
  __sync_fetch_and_nand (AL+15, 0xa00000007ll);



  __sync_add_and_fetch (AL+16, 1);

  __sync_add_and_fetch (AL+17, 0xb000e0000000ll);
  __sync_sub_and_fetch (AL+18, 22);
  __sync_sub_and_fetch (AL+19, 0xb000e0000000ll);

  __sync_and_and_fetch (AL+20, 0x300000007ll);
  __sync_or_and_fetch (AL+21, 0x500000009ll);
  __sync_xor_and_fetch (AL+22, 0xe00000001ll);
  __sync_nand_and_fetch (AL+23, 0xa00000007ll);
}


static void
do_ret_di (void)
{
  if (__sync_val_compare_and_swap (AL+0, 0x100000002ll, 0x1234567890ll) !=
 0x100000002ll) abort ();
  if (__sync_bool_compare_and_swap (AL+1, 0x200000003ll, 0x1234567890ll) !=
 1) abort ();
  if (__sync_lock_test_and_set (AL+2, 1) != 0) abort ();
  __sync_lock_release (AL+3);



  if (__sync_val_compare_and_swap (AL+4, 0x000000002ll, 0x1234567890ll) !=
 0x100000002ll) abort ();
  if (__sync_val_compare_and_swap (AL+5, 0x100000000ll, 0x1234567890ll) !=
 0x100000002ll) abort ();
  if (__sync_bool_compare_and_swap (AL+6, 0x000000002ll, 0x1234567890ll) !=
 0) abort ();
  if (__sync_bool_compare_and_swap (AL+7, 0x100000000ll, 0x1234567890ll) !=
 0) abort ();

  if (__sync_fetch_and_add (AL+8, 1) != 0) abort ();
  if (__sync_fetch_and_add (AL+9, 0xb000e0000000ll) != 0x1000e0de0000ll) abort ();
  if (__sync_fetch_and_sub (AL+10, 22) != 42) abort ();
  if (__sync_fetch_and_sub (AL+11, 0xb000e0000000ll) != 0xc001c0de0000ll)
 abort ();

  if (__sync_fetch_and_and (AL+12, 0x300000007ll) != -1ll) abort ();
  if (__sync_fetch_and_or (AL+13, 0x500000009ll) != 0) abort ();
  if (__sync_fetch_and_xor (AL+14, 0xe00000001ll) != 0xff00ff0000ll) abort ();
  if (__sync_fetch_and_nand (AL+15, 0xa00000007ll) != -1ll) abort ();



  if (__sync_add_and_fetch (AL+16, 1) != 1) abort ();
  if (__sync_add_and_fetch (AL+17, 0xb000e0000000ll) != 0xc001c0de0000ll)
 abort ();
  if (__sync_sub_and_fetch (AL+18, 22) != 20) abort ();
  if (__sync_sub_and_fetch (AL+19, 0xb000e0000000ll) != 0x1000e0de0000ll)
 abort ();

  if (__sync_and_and_fetch (AL+20, 0x300000007ll) != 0x300000007ll) abort ();
  if (__sync_or_and_fetch (AL+21, 0x500000009ll) != 0x500000009ll) abort ();
  if (__sync_xor_and_fetch (AL+22, 0xe00000001ll) != 0xf100ff0001ll) abort ();
  if (__sync_nand_and_fetch (AL+23, 0xa00000007ll) != ~0xa00000007ll) abort ();
}

int main ()
{
  memcpy (AL, init_di, sizeof (init_di));

  do_noret_di ();

  if (memcmp (AL, test_di, sizeof (test_di)))
    abort ();

  memcpy (AL, init_di, sizeof (init_di));

  do_ret_di ();

  if (memcmp (AL, test_di, sizeof (test_di)))
    abort ();

  return 0;
}
