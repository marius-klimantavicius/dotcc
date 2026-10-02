//type: fp
//options: 
# 0 "./uninit-pr111489.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./uninit-pr111489.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 5 "./uninit-pr111489.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 6 "./uninit-pr111489.c" 2


# 7 "./uninit-pr111489.c"
typedef struct
{
    uint16_t pgv1_size;
    uint16_t pgv1_flags1;
    uint16_t pgv1_flags2;
    uint16_t pgv1_flags3;
    uint16_t pgv1_nslots;
    uint32_t pgv1_generic;
} page_v1_t;

typedef struct
{
    page_v1_t pgv2_hdr;
    int64_t pgv2_next;
    int64_t pgv2_prev;
} page_v2_t;

typedef struct
{
    union
    {
        struct
        {
            uint16_t pgv0_size;
            uint16_t pgv0_flags;
        };
        uint32_t pgv0_nslots;
    };
    uint32_t pgv0_mode;
    uint32_t pgv0_next;
} page_v0_t;

typedef struct
{
    uint16_t sl4_off;
    uint16_t sl4_len;
} slot4_t;

typedef struct
{
    uint32_t sl8_off;
    uint32_t sl8_len;
} slot8_t;
extern int64_t cur_rowid;

extern uint8_t *pg_alloc(size_t pg_size);
extern void mem_move(void *dst, const void *src, size_t len);
extern void pg_expand(uint8_t *row_in, uint8_t *row_out, uint16_t pg_version);
extern void pg_reorg(uint32_t flags, uint8_t *pg_old, uint16_t slotnum, uint8_t *row_data, uint64_t cur_partp);

void pg_reorg(uint32_t flags, uint8_t *pg_old, uint16_t slotnum, uint8_t *row_data, uint64_t cur_partp)
{
    uint16_t pg_version;
    size_t pg_size = ((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800)));
    uint8_t *pg_new = pg_alloc(pg_size);
    uint8_t *old_slot = ((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (&(pg_old)[((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) - sizeof(int64_t) - (1*sizeof(slot8_t))]) : ((((page_v0_t *)(pg_old))->pgv0_mode & 1) ? (&(pg_old)[((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) - sizeof(int64_t) - (1*sizeof(slot4_t))]) : (&(pg_old)[((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) - sizeof(int32_t) - (1*sizeof(slot4_t))])));
    uint8_t *new_slot = ((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (&(pg_new)[((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) - sizeof(int64_t) - (1*sizeof(slot8_t))]) : ((((page_v0_t *)(pg_new))->pgv0_mode & 1) ? (&(pg_new)[((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) - sizeof(int64_t) - (1*sizeof(slot4_t))]) : (&(pg_new)[((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) - sizeof(int32_t) - (1*sizeof(slot4_t))])));

    if (flags == 2)
    {

        pg_version = ((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_generic >> 24) : ((uint32_t)((page_v0_t *)(pg_old))->pgv0_next >> 24));
    }

    int64_t sav_rowid = cur_rowid;


    for (uint16_t i = 1; i <= ((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_nslots) : (((page_v0_t *)(pg_old))->pgv0_nslots)); i++,
         (old_slot -= ((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? sizeof(slot8_t) : sizeof(slot4_t))), (new_slot -= ((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? sizeof(slot8_t) : sizeof(slot4_t))))
    {
        size_t length;


        if (flags == 1 && (slotnum == 0 || i == slotnum))
        {
            uint16_t slotlen = ((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(old_slot))->sl8_len & 0x000FFFFF) : (((slot4_t *)(old_slot))->sl4_len & 0x3FFF));
            ((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(new_slot))->sl8_len = (((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(new_slot))->sl8_len & 0xFFF00000) : (((slot4_t *)(new_slot))->sl4_len & 0xC000)) | ((slotlen) & 0x000FFFFF))) : (((slot4_t *)(new_slot))->sl4_len = (((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(new_slot))->sl8_len & 0xFFF00000) : (((slot4_t *)(new_slot))->sl4_len & 0xC000)) | ((slotlen) & 0x3FFF))));
            length = ((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(old_slot))->sl8_len & 0x000FFFFF) : (((slot4_t *)(old_slot))->sl4_len & 0x3FFF));
        }
        else
        {
            length = ((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(old_slot))->sl8_len & 0x000FFFFF) : (((slot4_t *)(old_slot))->sl4_len & 0x3FFF));
            ((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(new_slot))->sl8_len = (((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(new_slot))->sl8_len & 0xFFF00000) : (((slot4_t *)(new_slot))->sl4_len & 0xC000)) | ((length) & 0x000FFFFF))) : (((slot4_t *)(new_slot))->sl4_len = (((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)(new_slot))->sl8_len & 0xFFF00000) : (((slot4_t *)(new_slot))->sl4_len & 0xC000)) | ((length) & 0x3FFF))));
        }


        if (((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)old_slot)->sl8_off) : (((slot4_t *)old_slot)->sl4_off)) == 0)
        {
            ((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)new_slot)->sl8_off = (0)) : (((slot4_t *)new_slot)->sl4_off = (0)));
            continue;
        }

        ((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((slot8_t *)new_slot)->sl8_off = (0)) : (((slot4_t *)new_slot)->sl4_off = (0)));

        if (flags == 2)
        {
            cur_rowid = (((((cur_partp) & 0x400000000) ? 1 : 0)) ? (uint64_t)(((((int64_t)((((uint64_t)(sav_rowid) & ((uint64_t)1 << 63)) ? (int64_t)(((sav_rowid) & 0x7FFFFFFFFFFF0000) >> 16) : (int64_t)(((sav_rowid) & 0xFFFFFF00) >> 8)))) << 16) + (i)) | ((uint64_t)1 << 63)) : (uint64_t)((((int32_t)((((uint64_t)(sav_rowid) & ((uint64_t)1 << 63)) ? (int64_t)(((sav_rowid) & 0x7FFFFFFFFFFF0000) >> 16) : (int64_t)(((sav_rowid) & 0xFFFFFF00) >> 8)))) << 8) + (i)));
            pg_expand(((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (((uint8_t*)(pg_old)) + ((slot8_t *)old_slot)->sl8_off) : (((uint8_t*)(pg_old)) + ((slot4_t *)old_slot)->sl4_off)), row_data, pg_version);
        }
        else
        {

            mem_move(((((((page_v1_t *)(pg_new))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_new))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_new))->pgv0_size & 0xF800))) > 0x4000) ? (((uint8_t*)(pg_new)) + ((slot8_t *)new_slot)->sl8_off) : (((uint8_t*)(pg_new)) + ((slot4_t *)new_slot)->sl4_off)), ((((((page_v1_t *)(pg_old))->pgv1_flags3 == 3) ? (((page_v1_t *)(pg_old))->pgv1_size*1024) : (uint16_t)(0x0800 + (((page_v0_t *)(pg_old))->pgv0_size & 0xF800))) > 0x4000) ? (((uint8_t*)(pg_old)) + ((slot8_t *)old_slot)->sl8_off) : (((uint8_t*)(pg_old)) + ((slot4_t *)old_slot)->sl4_off)), length);
        }
    }
}
