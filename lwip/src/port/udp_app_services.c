/* Authored owner boundary, compiled instead of the standalone original tftp.c.
 * The include path selects pinned/staged source without editing it. Preparation
 * must guard the original entry body, state and close-call context contracts.
 */
#include "udp_app_services.h"
#include "apps/tftp/tftp.c"

#if LWIP_UDP
static u8_t lwip_owned_tftp_active;
static unsigned int lwip_owned_tftp_dispatch;

static void lwip_owned_tftp_receive(void *arg,struct udp_pcb *pcb,struct pbuf *p,const ip_addr_t *address,u16_t port)
{
  unsigned int previous=lwip_owned_tftp_dispatch;
  unsigned char opcode[2];
  lwip_owned_tftp_dispatch=0;
  if(pbuf_copy_partial(p,opcode,2,0)==2)
    lwip_owned_tftp_dispatch=((unsigned int)opcode[0]<<8)|opcode[1];
  tftp_recv(arg,pcb,p,address,port); /* Original parser, validation and ownership. */
  lwip_owned_tftp_dispatch=previous;
}

err_t lwip_owned_tftp_start(u8_t mode,const struct tftp_context *context)
{
  LWIP_ASSERT_CORE_LOCKED();
  if(lwip_owned_tftp_active||tftp_state.upcb!=NULL)return ERR_USE;
  if(context==NULL||(mode!=LWIP_TFTP_MODE_SERVER&&mode!=LWIP_TFTP_MODE_CLIENT))return ERR_ARG;
  err_t result=tftp_init_common(mode,context);
  if(result==ERR_OK) {
    lwip_owned_tftp_active=1;
    udp_recv(tftp_state.upcb,lwip_owned_tftp_receive,NULL);
  }
  return result;
}

void lwip_owned_tftp_stop(void)
{
  LWIP_ASSERT_CORE_LOCKED();
  if(!lwip_owned_tftp_active)return;
  unsigned int previous=lwip_owned_tftp_dispatch;
  lwip_owned_tftp_dispatch=256;
  tftp_cleanup(); /* Cancels original timer, releases pbuf and invokes close. */
  lwip_owned_tftp_dispatch=previous;
  lwip_owned_tftp_active=0;
}

unsigned int lwip_owned_tftp_close_context(void)
{ return lwip_owned_tftp_dispatch; }

/* Guarded replacement only for the tiny public PUT entry. Validators and
 * request serialization are the original contract; the server's initial ACK is
 * block zero, so initialize before sending and let the unchanged ACK handler
 * advance to data block one. No after-send state mutation or parser replacement.
 */
err_t lwip_host_tftp_put(void* handle,const ip_addr_t *addr,u16_t port,const char* fname,enum tftp_transfer_mode mode)
{
  LWIP_ERROR("TFTP client is not enabled (tftp_init)", (tftp_state.tftp_mode & LWIP_TFTP_MODE_CLIENT) != 0, return ERR_VAL);
  LWIP_ERROR("tftp_put: invalid file name", fname != NULL, return ERR_VAL);
  LWIP_ERROR("tftp_put: invalid mode", mode <= TFTP_MODE_BINARY, return ERR_VAL);

  tftp_state.handle = handle;
  tftp_state.blknum = 0;
  tftp_state.mode_write = 0;
  return send_request(addr, port, TFTP_WRQ, fname, mode_to_string(mode));
}
#endif
