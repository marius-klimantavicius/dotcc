#ifndef DOTCC_TCP_APPLICATION_NATIVE_H
#define DOTCC_TCP_APPLICATION_NATIVE_H
#include "lwip/netif.h"
#include "lwip/tcp.h"
#include "lwip/pbuf.h"
#include "lwip/sys.h"
#include "lwip/timeouts.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdatomic.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x);exit(92);}}while(0)
extern int lwip_host_call(void (*)(void *),void *);
extern void lwip_host_poll(void);
extern int lwip_host_barrier(void);
extern ip_addr_t fixture_address;
extern char fixture_observations[512];
void fixture_wait(atomic_int *);
void fixture_wait_checked(atomic_int *,void (*)(void *));
void fixture_advance_time(u32_t);
void fixture_mqtt(void);
void fixture_smtp(void);
void fixture_lwiperf(void);
void fixture_http(void);
void fixture_proxy(void);
/* These peer calls execute on the original core. The wait/capture helpers marshal
 * their own core callbacks; no original raw API is called off-core. */
void peer_start(u16_t);
void peer_destroy(void);
void peer_send(const void *,size_t);
void peer_finish_send(void);
void peer_set_discard(int);
void peer_set_capture_limit(size_t);
size_t peer_received(void);
extern atomic_int peer_connected,peer_finished;
extern unsigned char peer_message[262144];
extern size_t peer_message_length;
void peer_wait_mqtt(void);
void peer_wait_until(const char *);
void peer_wait_bytes(size_t);
void peer_send_sync(const void *,size_t);
void peer_text_sync(const char *);
void peer_finish_sync(void);
void peer_destroy_sync(void);
static inline unsigned get16(const unsigned char *p){return ((unsigned)p[0]<<8)|p[1];}
static inline unsigned get32(const unsigned char *p){return ((unsigned)p[0]<<24)|((unsigned)p[1]<<16)|((unsigned)p[2]<<8)|p[3];}
#endif
