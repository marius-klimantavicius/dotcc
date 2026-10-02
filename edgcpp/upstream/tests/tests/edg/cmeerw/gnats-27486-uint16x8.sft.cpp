//type:fp
//options:--gn 140100 --target linux_aarch64
//options_all:--c++

void f(__Uint16x8_t i)
{
  __Uint16x8_t val = __builtin_aarch64_bswapv8hi_uu(i);
}
