/* Same source is compiled natively and translated; instruction decoding only. */
#include <stdio.h>
#include <stddef.h>
#include <string.h>
#include "blink/x86.h"

static const unsigned char cases[][15] = {
  {0x90}, {0x48,0xb8,1,2,3,4,5,6,7,8}, {0x48,0x01,0xd8},
  {0xf3,0xa4}, {0x66,0x0f,0xef,0xc0}, {0x0f,0x05}, {0x0f,0x0b},
  {0x48,0x8b,0x84,0x88,0x78,0x56,0x34,0x12}, {0xeb,0xfe},
  {0x48,0x83,0xc0,0xff}, {0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66,0x66}
};
static const unsigned char lengths[] = {1,10,3,2,4,2,2,8,2,4,15};
int main(void) {
  unsigned i;
  struct XedMachineMode mode = {2,1};
  printf("ABI mode=%u operands=%u decoded=%u op-offset=%u mode-byte=%u\n",
         (unsigned)sizeof(mode), (unsigned)sizeof(struct XedOperands),
         (unsigned)sizeof(struct XedDecodedInst),
         (unsigned)offsetof(struct XedDecodedInst,op), *(unsigned char *)&mode);
  for (i=0;i<sizeof(lengths);++i) {
    struct XedDecodedInst decoded;
    int error;
    memset(&decoded,0,sizeof(decoded));
    error=DecodeInstruction(&decoded,cases[i],lengths[i],XED_MODE_LONG);
    printf("case=%u error=%d length=%u rde=%llu immediate=%llu disp=%lld\n", i,error,
           (unsigned)decoded.length, (unsigned long long)decoded.op.rde,
           (unsigned long long)decoded.op.uimm0,(long long)decoded.op.disp);
  }
  return 0;
}
