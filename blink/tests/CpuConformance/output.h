#ifndef BLINK_CPU_CONFORMANCE_OUTPUT_H
#define BLINK_CPU_CONFORMANCE_OUTPUT_H
#include <stdio.h>
#include <inttypes.h>
struct CpuResult {
  uint64_t ax,bx,cx,dx,flags;
  unsigned mxcsr;
  int64_t ip;
  int signal,raw_signal,raw_code,halt,completed;
  unsigned char xmm[32];
};
static void CpuHex(const unsigned char *bytes,size_t count) {
  for(size_t i=0;i<count;++i)printf("%02x",bytes[i]);
}
static inline void CpuPrint(const struct CpuCase *c,const struct CpuResult *r,
                     const unsigned char *data,size_t size) {
  printf("{\"name\":\"%s\",\"ax\":\"%016" PRIx64 "\",\"cx\":\"%016" PRIx64
         "\",\"dx\":\"%016" PRIx64 "\",\"flags\":\"%016" PRIx64
         "\",\"flagMask\":\"%016" PRIx64 "\",\"ip\":%" PRId64
         ",\"signal\":%d,\"rawSignal\":%d,\"rawCode\":%d,\"halt\":%d,\"completed\":%d,\"xmm\":\"",
         c->name,r->ax,r->cx,r->dx,r->flags,c->flag_mask,r->ip,
         r->signal,r->raw_signal,r->raw_code,r->halt,r->completed);
  CpuHex(r->xmm,sizeof(r->xmm));printf("\",\"bx\":\"%016" PRIx64 "\",\"mxcsr\":%u,\"memory\":\"",r->bx,r->mxcsr);
  CpuHex(data,size);printf("\"}\n");
}
#endif
