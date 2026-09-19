#include "corpus.h"
#include "output.h"
int main(void) {
  unsigned char data[2*CPU_PAGE];CpuData(data,sizeof(data));
  printf("{\"schema\":1,\"pageSize\":4096,\"xmmInput\":\"");CpuHex(cpu_xmm,sizeof(cpu_xmm));
  printf("\",\"dataInput\":\"");CpuHex(data,sizeof(data));printf("\",\"cases\":[");
  for(unsigned i=0;i<CPU_CASES;++i){
    const struct CpuCase *c=cpu_cases+i;
    if(i)putchar(',');
    printf("{\"index\":%u,\"name\":\"%s\",\"code\":\"",i,c->name);CpuHex(c->code,c->length);
    printf("\",\"steps\":%u,\"codeOffset\":%u,\"dataOffset\":%u,\"dataPages\":%u,"
      "\"ax\":\"%016" PRIx64 "\",\"cx\":\"%016" PRIx64 "\",\"dx\":\"%016" PRIx64
      "\",\"flags\":\"%016" PRIx64 "\",\"flagMask\":\"%016" PRIx64 "\",\"fault\":%d}",
      c->steps,c->code_offset,c->data_offset,c->data_pages,c->ax,c->cx,c->dx,c->flags,c->flag_mask,c->fault);
  }
  printf("]}\n");return 0;
}
