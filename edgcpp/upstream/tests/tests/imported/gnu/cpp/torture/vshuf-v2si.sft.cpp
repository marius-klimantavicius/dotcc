//type: rp
//options:  --c++11
# 0 "./torture/vshuf-v2si.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/vshuf-v2si.C"



typedef unsigned int V __attribute__((vector_size(8)));
typedef V VI;
# 19 "./torture/vshuf-v2si.C"
# 1 "./torture/vshuf-2.inc" 1




constexpr V in1[] = { { 0x11121314, 0x21222324}, {0x11121314, 0x21222324}, { 0x11121314, 0x21222324},
            { 0x11121314, 0x21222324}, {0xd1d2d3d4, 0xe1e2e3e4}, { 0xd1d2d3d4, 0xe1e2e3e4}};
constexpr VI mask1[] = { {0, 1}, {(unsigned)-16, 1}, {1, 0},
                {0, 0}, { 1, 1}, {1, 0}};
constexpr V out1[] = { {0x11121314, 0x21222324}, {0x11121314, 0x21222324}, {0x21222324, 0x11121314},
             {0x11121314, 0x11121314}, {0xe1e2e3e4, 0xe1e2e3e4}, {0xe1e2e3e4, 0xd1d2d3d4}};

constexpr V in2[] = { { 0x11121314, 0x21222324}, {0x11121314, 0x21222324}, {0x11121314, 0x21222324}, {0x11121314, 0x21222324},
            { 0x11121314, 0x21222324}, {0x11121314, 0x21222324}, {0x11121314, 0x21222324}};
constexpr V in3 = {0xd1d2d3d4, 0xe1e2e3e4};
constexpr VI mask2[] = { {0, 1}, {2, 3}, {0, 2}, {2, 1},
                {3, 0}, {0, 0}, {3, 3}};

constexpr V out2[] = { {0x11121314, 0x21222324}, {0xd1d2d3d4, 0xe1e2e3e4}, {0x11121314, 0xd1d2d3d4}, {0xd1d2d3d4, 0x21222324},
             {0xe1e2e3e4, 0x11121314}, {0x11121314, 0x11121314}, {0xe1e2e3e4, 0xe1e2e3e4}};
# 20 "./torture/vshuf-v2si.C" 2
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
# 21 "./torture/vshuf-v2si.C" 2
