/* Application data/storage policy through upstream application callback contracts. */
#include "lwip/apps/fs.h"
#include "lwip/apps/httpd.h"
#include "lwip/apps/snmpv3.h"
#include "lwip/pbuf.h"
#include "lwip/sys.h"
#include <string.h>
extern const unsigned char *lwip_host_http_open(const char *name, int *length);
extern void lwip_host_http_close(const unsigned char *data);
extern int lwip_host_post_begin(void *connection, const char *uri, int length);
extern int lwip_host_post_data(void *connection, const unsigned char *data, unsigned int length);
extern void lwip_host_post_finish(void *connection, char *uri, unsigned int capacity);
#if LWIP_HTTPD_CUSTOM_FILES
int fs_open_custom(struct fs_file *file, const char *name)
{
  int length=0; const unsigned char *data=lwip_host_http_open(name,&length);
  if(!data)return 0;
  memset(file,0,sizeof(*file));file->data=(const char*)data;file->len=length;file->index=length;
  file->flags=FS_FILE_FLAGS_CUSTOM;return 1;
}
void fs_close_custom(struct fs_file *file){lwip_host_http_close((const unsigned char*)file->data);file->data=NULL;}
#endif
#if LWIP_HTTPD_SUPPORT_POST
err_t httpd_post_begin(void *connection,const char *uri,const char *request,u16_t request_len,int content_len,char *response,u16_t response_len,u8_t *auto_wnd)
{ LWIP_UNUSED_ARG(request);LWIP_UNUSED_ARG(request_len);LWIP_UNUSED_ARG(response);LWIP_UNUSED_ARG(response_len);*auto_wnd=1;return lwip_host_post_begin(connection,uri,content_len)?ERR_OK:ERR_ARG; }
err_t httpd_post_receive_data(void *connection,struct pbuf *p)
{ err_t result=ERR_OK; for(struct pbuf *q=p;q;q=q->next)if(!lwip_host_post_data(connection,q->payload,q->len))result=ERR_ARG;pbuf_free(p);return result; }
void httpd_post_finished(void *connection,char *response,u16_t capacity){lwip_host_post_finish(connection,response,capacity);}
#endif
#if LWIP_SNMP && LWIP_SNMP_V3
/* Per-instance volatile noAuthNoPriv configuration; no external crypto provider. */
static char engine_id[32]={0x00,0x00,0x00,0x00,0x05,'D','o','t','C','C'};
static u8_t engine_length=10;
static u32_t engine_boots=1,engine_start;
static char noauth_username[33];
void snmpv3_get_engine_id(const char **id,u8_t *len){*id=engine_id;*len=engine_length;}
err_t snmpv3_set_engine_id(const char *id,u8_t len){if(len<5||len>32)return ERR_ARG;memcpy(engine_id,id,len);engine_length=len;snmpv3_engine_id_changed();return ERR_OK;}
u32_t snmpv3_get_engine_boots(void){return engine_boots;}
void snmpv3_set_engine_boots(u32_t boots){engine_boots=boots;}
u32_t snmpv3_get_engine_time(void){return (sys_now()-engine_start)/1000;}
void snmpv3_reset_engine_time(void){engine_start=sys_now();}
int lwip_host_snmp_set_noauth_user(const char *name){size_t n=strlen(name);if(n>32)return 0;memcpy(noauth_username,name,n+1);return 1;}
err_t snmpv3_get_user(const char *username,snmpv3_auth_algo_t *auth,u8_t *auth_key,snmpv3_priv_algo_t *priv,u8_t *priv_key)
{
  LWIP_UNUSED_ARG(auth_key);LWIP_UNUSED_ARG(priv_key);
  if(!noauth_username[0]||strcmp(username,noauth_username))return ERR_VAL;
  if(auth)*auth=SNMP_V3_AUTH_ALGO_INVAL;if(priv)*priv=SNMP_V3_PRIV_ALGO_INVAL;return ERR_OK;
}
u8_t snmpv3_get_amount_of_users(void){return noauth_username[0]?1:0;}
err_t snmpv3_get_username(char *username,u8_t index){if(index||!noauth_username[0])return ERR_VAL;strcpy(username,noauth_username);return ERR_OK;}
err_t snmpv3_get_user_storagetype(const char *username,snmpv3_user_storagetype_t *storage){if(snmpv3_get_user(username,NULL,NULL,NULL,NULL))return ERR_VAL;*storage=SNMP_V3_USER_STORAGETYPE_VOLATILE;return ERR_OK;}
#endif
