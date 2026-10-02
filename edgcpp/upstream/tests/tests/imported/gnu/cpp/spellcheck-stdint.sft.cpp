//type: fn
//options:  --c++11
# 0 "./spellcheck-stdint.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./spellcheck-stdint.C"



char c = INT8_MAX;


short s = INT16_MAX;


int i = INT32_MAX;


long l = INT64_MAX;


intptr_t test_intptr (void)

{
  return 0;
}

int test_intptr_max (void)
{
  return (int) INTPTR_MAX;

}

uintptr_t test_uintptr (void)

{
  return 0;
}

unsigned int test_uintptr_max (void)
{
  return (unsigned int) UINTPTR_MAX;

}

int8_t i8;

int16_t i16;

int32_t i32;

int64_t i64;


void test_uint_t (void)
{
  char bu8[(unsigned int)UINT8_MAX];

  char bu16[(unsigned int)UINT16_MAX];

  char bu32[(unsigned int)UINT32_MAX];

  char bu64[(unsigned int)UINT64_MAX];


  auto ui8 = (uint8_t) 8;

  auto ui16 = (uint16_t) 16;

  auto ui32 = (uint32_t) 32;

  auto ui64 = (uint64_t) 64;

}
