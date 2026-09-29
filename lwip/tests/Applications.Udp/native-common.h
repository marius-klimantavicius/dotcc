#ifndef DOTCC_UDP_NATIVE_COMMON_H
#define DOTCC_UDP_NATIVE_COMMON_H
#include "lwip/netif.h"
#include "lwip/udp.h"
#include "lwip/pbuf.h"
#include "lwip/sys.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdatomic.h>
#include <time.h>
#define R(x) do { if (!(x)) { fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x); exit(92); } } while (0)
extern int lwip_host_call(void (*)(void *),void *);
extern void lwip_host_poll(void);
extern int lwip_host_barrier(void);
extern struct netif *fixture_interface;
extern ip_addr_t fixture_address;
extern char fixture_observations[256];
void fixture_send(struct udp_pcb *,u16_t,const unsigned char *,size_t);
void fixture_wait(atomic_int *);
void fixture_wait_checked(atomic_int *,void (*)(void *));
unsigned fixture_udp_count(void);
void fixture_tftp(const char *);
void fixture_sntp(void);
void fixture_netbios(void);
void fixture_mdns(void);
void fixture_owned_tftp(void);
void fixture_advance_time(u32_t);
static inline unsigned get16(const unsigned char *p) { return ((unsigned)p[0]<<8)|p[1]; }
static inline void put16(unsigned char *p,unsigned n) { p[0]=(unsigned char)(n>>8);p[1]=(unsigned char)n; }
static inline void put32(unsigned char *p,u32_t n) { p[0]=(unsigned char)(n>>24);p[1]=(unsigned char)(n>>16);p[2]=(unsigned char)(n>>8);p[3]=(unsigned char)n; }
#endif
