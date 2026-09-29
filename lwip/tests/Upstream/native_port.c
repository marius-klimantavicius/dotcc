#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <check.h>
uint32_t lwip_host_random(void) { return (uint32_t)rand(); }
void lwip_host_assert(const char *message,const char *file,int line)
{
  fprintf(stderr,"lwIP assertion %s:%d: %s\n",file,line,message);
  ck_abort_msg("lwIP assertion %s:%d: %s",file,line,message);
}
