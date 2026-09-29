#include "native-common.h"
#include "lwip/apps/smtp.h"
static atomic_int result_done;
static u8_t result_code;
static u16_t server_code;
static err_t transport_code;
static unsigned calls;
struct scenario{u16_t port;int retained;};
static void result(void *arg,u8_t code,u16_t server,err_t error)
{R(arg==&result_done);calls++;R(calls==1);result_code=code;server_code=server;transport_code=error;atomic_store(&result_done,1);}
static void begin(void *arg)
{
  struct scenario *test=arg;peer_start(test->port);calls=0;atomic_store(&result_done,0);
  R(smtp_set_server_addr("10.0.0.1")==ERR_OK);smtp_set_server_port(test->port);R(smtp_set_auth(NULL,NULL)==ERR_OK);
  err_t err=test->retained?smtp_send_mail_static("sender@fixture.test","reader@fixture.test","original SMTP fixture","line one\r\nline two",result,&result_done)
    :smtp_send_mail("sender@fixture.test","reader@fixture.test","original SMTP fixture","line one\r\nline two",result,&result_done);
  if(err!=ERR_OK)fprintf(stderr,"original SMTP submission returned %d for retained=%d port=%u\n",(int)err,test->retained,test->port);
  R(err==ERR_OK);
}
static void line(const char *expected){peer_wait_until("\r\n");R(peer_message_length==strlen(expected)&&!memcmp(peer_message,expected,peer_message_length));}
static void envelope(void)
{
  fixture_wait(&peer_connected);peer_text_sync("220 fixture.test service ready\r\n");line("EHLO [10.0.0.1]\r\n");
  peer_text_sync("250 fixture.test\r\n");line("MAIL FROM: <sender@fixture.test>\r\n");peer_text_sync("250 sender accepted\r\n");line("RCPT TO: <reader@fixture.test>\r\n");
}
void fixture_smtp(void)
{
  for(unsigned i=0;i<4;i++)
  {
    struct scenario test={(u16_t)(2525+i),(i==1||i==3)};R(lwip_host_call(begin,&test)==0);
    if(i==3){fixture_wait(&peer_connected);peer_finish_sync();fixture_wait(&result_done);R(result_code==SMTP_RESULT_ERR_CLOSED);}
    else
    {
      envelope();
      if(i==2){peer_text_sync("550 Requested action not taken: mailbox unavailable\r\n");fixture_wait(&result_done);R(result_code==SMTP_RESULT_ERR_SVR_RESP&&server_code==550);}
      else
      {
        peer_text_sync("250 recipient accepted\r\n");line("DATA\r\n");peer_text_sync("354 End with CRLF dot CRLF\r\n");peer_wait_until("\r\n.\r\n");
        static const char mail[]="From: <sender@fixture.test>\r\nTo: <reader@fixture.test>\r\nSubject: original SMTP fixture\r\n\r\nline one\r\nline two\r\n.\r\n";
        R(peer_message_length==sizeof(mail)-1&&!memcmp(peer_message,mail,sizeof(mail)-1));
        peer_text_sync("250 message accepted\r\n");line("QUIT\r\n");fixture_wait(&result_done);R(result_code==SMTP_RESULT_OK&&server_code==0&&transport_code==ERR_OK);
        peer_text_sync("221 closing connection\r\n");peer_finish_sync();
      }
    }
    fixture_wait(&peer_finished);R(calls==1);peer_destroy_sync();
  }
  snprintf(fixture_observations,sizeof(fixture_observations),",\"copied_mail\":true,\"retained_static_mail\":true,\"recipient_550\":true,\"ordinary_remote_close\":true");
}
