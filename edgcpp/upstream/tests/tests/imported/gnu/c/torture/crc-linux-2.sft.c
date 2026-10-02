//type: fp
//options: 
# 0 "./torture/crc-linux-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/crc-linux-2.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

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
# 6 "./torture/crc-linux-2.c" 2

# 6 "./torture/crc-linux-2.c"
typedef unsigned char u8;
typedef unsigned short u16;
typedef unsigned char __u8;
typedef unsigned short __u16;
struct i2c_msg {
    __u16 addr;
    __u16 flags;


    __u16 len;
    __u8 *buf;
};


static u8 crc8(u16 data)
{
  int i;

  for (i = 0; i < 8; i++) {
      if (data & 0x8000)
 data = data ^ (0x1070U << 3);
      data = data << 1;
    }
  return (u8)(data >> 8);
}
# 40 "./torture/crc-linux-2.c"
u8 i2c_smbus_pec(u8 crc, u8 *p, size_t count)
{
  int i;

  for (i = 0; i < count; i++)
    crc = crc8((crc ^ p[i]) << 8);
  return crc;
}
static inline u8 i2c_8bit_addr_from_msg(const struct i2c_msg *msg)
{
  return (msg->addr << 1) | (msg->flags & 0x0001 ? 1 : 0);
}



u8 i2c_smbus_msg_pec(u8 pec, struct i2c_msg *msg)
{

  u8 addr = i2c_8bit_addr_from_msg(msg);
  pec = i2c_smbus_pec(pec, &addr, 1);


  return i2c_smbus_pec(pec, msg->buf, msg->len);
}
