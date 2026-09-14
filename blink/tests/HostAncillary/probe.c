#define _GNU_SOURCE 1
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <sys/socket.h>
#ifdef BLINK_PRIVATE_ANCILLARY
#include "HostAncillary.c"
#define Next(m,c) blink_host_cmsg_nxthdr(m,c)
#else
#define Next(m,c) CMSG_NXTHDR(m,c)
#endif
#define CHECK(x) do { if(!(x)){printf("ancillary failure %d\n",__LINE__);return __LINE__;} } while(0)
int AncillaryProbe(void) {
  union { size_t aligned; unsigned char bytes[160]; } storage;
  struct msghdr message={0};
  unsigned long digest=1469598103934665603UL;
  unsigned long cases=0;
  memset(&storage,0,sizeof(storage));message.msg_control=storage.bytes;
  for(size_t count=sizeof(struct cmsghdr);count<=128;++count) {
    message.msg_controllen=count;
    for(size_t offset=0;offset+sizeof(struct cmsghdr)<=count;offset+=sizeof(size_t)) {
      struct cmsghdr *current=(struct cmsghdr *)(storage.bytes+offset);
      for(size_t length=0;length<=160;++length) {
        current->cmsg_len=length;errno=123;
        struct cmsghdr *next=Next(&message,current);
        CHECK(errno==123);
        unsigned long value=next?(unsigned long)((unsigned char *)next-storage.bytes)+1:0;
        digest=(digest^value)*1099511628211UL;++cases;
      }
      current->cmsg_len=SIZE_MAX;CHECK(!Next(&message,current));
    }
  }
  printf("control-record traversal cases=%lu digest=%lu header=%lu align=%lu\n",cases,digest,(unsigned long)sizeof(struct cmsghdr),(unsigned long)sizeof(size_t));
#ifdef BLINK_PRIVATE_ANCILLARY
  struct cmsghdr *(*call)(const struct msghdr *,const struct cmsghdr *)=blink_host_cmsg_nxthdr;
  struct cmsghdr *current=(struct cmsghdr *)storage.bytes;
  CHECK(!call(0,current) && !call(&message,0));
  message.msg_controllen=15;CHECK(!call(&message,current));
  message.msg_control=storage.bytes+16;message.msg_controllen=32;CHECK(!call(&message,current));
  message.msg_control=0;CHECK(!call(&message,current));
#endif
  return 0;
}
#ifndef BLINK_MANAGED_ANCILLARY
int main(void){return AncillaryProbe();}
#endif
