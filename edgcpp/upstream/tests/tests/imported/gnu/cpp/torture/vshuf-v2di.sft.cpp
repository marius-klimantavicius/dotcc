//type: rp
//options:  --c++11
# 0 "./torture/vshuf-v2di.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/vshuf-v2di.C"




typedef unsigned long long V __attribute__((vector_size(16)));
typedef V VI;
# 17 "./torture/vshuf-v2di.C"
# 1 "./torture/vshuf-2.inc" 1




constexpr V in1[] = { { 0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728}, { 0x1112131415161718, 0x2122232425262728},
            { 0x1112131415161718, 0x2122232425262728}, {0xc1c2c3c4c5c6c7c8, 0xd1d2d3d4d5d6d7d8}, { 0xc1c2c3c4c5c6c7c8, 0xd1d2d3d4d5d6d7d8}};
constexpr VI mask1[] = { {0, 1}, {(unsigned)-16, 1}, {1, 0},
                {0, 0}, { 1, 1}, {1, 0}};
constexpr V out1[] = { {0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728}, {0x2122232425262728, 0x1112131415161718},
             {0x1112131415161718, 0x1112131415161718}, {0xd1d2d3d4d5d6d7d8, 0xd1d2d3d4d5d6d7d8}, {0xd1d2d3d4d5d6d7d8, 0xc1c2c3c4c5c6c7c8}};

constexpr V in2[] = { { 0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728},
            { 0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728}, {0x1112131415161718, 0x2122232425262728}};
constexpr V in3 = {0xc1c2c3c4c5c6c7c8, 0xd1d2d3d4d5d6d7d8};
constexpr VI mask2[] = { {0, 1}, {2, 3}, {0, 2}, {2, 1},
                {3, 0}, {0, 0}, {3, 3}};

constexpr V out2[] = { {0x1112131415161718, 0x2122232425262728}, {0xc1c2c3c4c5c6c7c8, 0xd1d2d3d4d5d6d7d8}, {0x1112131415161718, 0xc1c2c3c4c5c6c7c8}, {0xc1c2c3c4c5c6c7c8, 0x2122232425262728},
             {0xd1d2d3d4d5d6d7d8, 0x1112131415161718}, {0x1112131415161718, 0x1112131415161718}, {0xd1d2d3d4d5d6d7d8, 0xd1d2d3d4d5d6d7d8}};
# 18 "./torture/vshuf-v2di.C" 2
# 1 "./torture/vshuf-main.inc" 1





extern "C" void abort(void);

int main()
{

  int i;

  for (i = 0; i < sizeof(in1)/sizeof(in1[0]); ++i)
    {
      V r = __builtin_shuffle(in1[i], mask1[i]);
      if (__builtin_memcmp(&r, &out1[i], sizeof(V)) != 0)
 abort();
    }

  for (i = 0; i < sizeof(in2)/sizeof(in2[0]); ++i)
    {
      V r = __builtin_shuffle(in2[i], in3, mask2[i]);
      if (__builtin_memcmp(&r, &out2[i], sizeof(V)) != 0)
 abort();
    }


  return 0;
}
# 19 "./torture/vshuf-v2di.C" 2
