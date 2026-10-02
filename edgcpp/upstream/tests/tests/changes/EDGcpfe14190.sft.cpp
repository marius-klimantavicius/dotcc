//type:fp
//options_all:--g++
//remark:[4.8] GNU compatibility: __builtin_bswap16
// 6/28/13  [EDGcpfe/14190]
//
// GNU compatibility: __builtin_bswap16
//
// In GNU modes, the front end now predeclares the byte swapping function
// __builtin_bswap16.
typedef unsigned short uint16_t;
int main() {
  return __builtin_bswap16((uint16_t)0xaabb) != (uint16_t)0xbbaa;
}
