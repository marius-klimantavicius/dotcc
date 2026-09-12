/* Native semantic control for the authored post-handshake helper. The opaque
 * ticket-ID store below is test-only, in-process, and never a ticket protector. */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <openssl/pem.h>
#include <openssl/x509.h>
#include <picotls.h>
#include <picotls/openssl.h>

int dotcc_ptls_send_quic_ticket(ptls_t *, ptls_buffer_t *);
#define CHECK(x) do { if (!(x)) { fprintf(stderr,"native helper check failed at %d: %s\n",__LINE__,#x); exit(1); } } while (0)
static uint64_t now_ms;
static unsigned dh_creates, dh_exchanges;
static int fail_ticket;
static uint64_t clock_cb(ptls_get_time_t *self) { (void)self; return now_ms; }
static ptls_get_time_t clock_source = {clock_cb};
static int create_dh(const struct st_ptls_key_exchange_algorithm_t *a, ptls_key_exchange_context_t **c)
{ (void)a; ++dh_creates; return ptls_openssl_secp256r1.create(&ptls_openssl_secp256r1,c); }
static int exchange_dh(const struct st_ptls_key_exchange_algorithm_t *a, ptls_iovec_t *p, ptls_iovec_t *s, ptls_iovec_t peer)
{ (void)a; ++dh_exchanges; return ptls_openssl_secp256r1.exchange(&ptls_openssl_secp256r1,p,s,peer); }
static ptls_key_exchange_algorithm_t observed_dh = {23,create_dh,exchange_dh,0,"secp256r1"};
static ptls_key_exchange_algorithm_t *exchanges[] = {&observed_dh,NULL};
typedef struct { uint8_t id[16]; uint8_t *session; size_t length; } stored_t;
static stored_t store[32]; static size_t stored_count;
static int push(ptls_buffer_t *out, const void *bytes, size_t length)
{
    int ret = ptls_buffer_reserve(out,length); if(ret) return ret;
    memcpy(out->base+out->off,bytes,length); out->off+=length; return 0;
}
static int protect(ptls_encrypt_ticket_t *self, ptls_t *tls, int enc, ptls_buffer_t *out, ptls_iovec_t input)
{
    (void)self; (void)tls;
    if(enc) {
        if(fail_ticket) return PTLS_ERROR_NO_MEMORY;
        CHECK(stored_count<32); stored_t *entry=&store[stored_count++];
        ptls_openssl_random_bytes(entry->id,16); entry->session=malloc(input.len); CHECK(entry->session);
        memcpy(entry->session,input.base,input.len); entry->length=input.len;
        return push(out,entry->id,16);
    }
    for(size_t i=0;i<stored_count;i++) if(input.len==16 && memcmp(input.base,store[i].id,16)==0) {
        int ret=push(out,store[i].session,store[i].length);
        return ret?ret:PTLS_ERROR_REJECT_EARLY_DATA;
    }
    return PTLS_ERROR_SESSION_NOT_FOUND;
}
static ptls_encrypt_ticket_t protector = {protect};
typedef struct {
    ptls_t *tls; ptls_handshake_properties_t props;
    uint8_t *saved[8]; size_t saved_length[8], saved_count;
    unsigned keys; uint8_t secrets[2][2][48]; size_t secret_length;
} peer_t;
static peer_t *peer(ptls_t *tls) { return *(peer_t **)ptls_get_data_ptr(tls); }
static int save_ticket(ptls_save_ticket_t *self, ptls_t *tls, ptls_iovec_t input, const ptls_save_ticket_properties_t *properties)
{
    (void)self; CHECK(properties->early_data==0 && properties->max_early_data_size==0);
    peer_t *p=peer(tls); CHECK(p->saved_count<8); size_t i=p->saved_count++;
    p->saved[i]=malloc(input.len); CHECK(p->saved[i]); memcpy(p->saved[i],input.base,input.len); p->saved_length[i]=input.len; return 0;
}
static ptls_save_ticket_t saver={save_ticket};
static int traffic(ptls_update_traffic_key_t *self, ptls_t *tls, int enc, size_t epoch, const void *secret)
{
    (void)self; CHECK(epoch==2||epoch==3); CHECK(enc==0||enc==1);
    peer_t *p=peer(tls); size_t n=ptls_get_cipher(tls)->hash->digest_size; CHECK(n==32||n==48);
    CHECK(!(p->keys&(1u<<((epoch-2)*2+enc)))); p->keys|=1u<<((epoch-2)*2+enc);
    memcpy(p->secrets[epoch-2][enc],secret,n); p->secret_length=n; return 0;
}
static ptls_update_traffic_key_t traffic_keys={traffic};
static int hello(ptls_on_client_hello_t *self, ptls_t *tls, ptls_on_client_hello_parameters_t *params)
{
    (void)self;
    if(params->server_name.len) CHECK(ptls_set_server_name(tls,(const char *)params->server_name.base,params->server_name.len)==0);
    return ptls_set_negotiated_protocol(tls,"helper",6);
}
static ptls_on_client_hello_t hello_cb={hello};
static ptls_iovec_t alpn;
static void initialize(peer_t *p,ptls_context_t *ctx,int server,ptls_iovec_t ticket)
{
    memset(p,0,sizeof(*p)); p->tls=ptls_new(ctx,server); CHECK(p->tls);
    *ptls_get_data_ptr(p->tls)=p;
    if(!server) {
        CHECK(ptls_set_server_name(p->tls,"localhost",0)==0);
        p->props.client.negotiated_protocols.list=&alpn; p->props.client.negotiated_protocols.count=1;
        p->props.client.session_ticket=ticket;
    }
}
typedef struct { peer_t *receiver; size_t epoch,length; uint8_t *bytes; } flight_t;
static flight_t flights[64]; static size_t head,tail;
static void process(peer_t *sender,peer_t *receiver,size_t epoch,const uint8_t *input,size_t length)
{
    uint8_t small[4096]; ptls_buffer_t out; ptls_buffer_init(&out,small,sizeof(small)); size_t offsets[5]={0};
    CHECK(ptls_get_read_epoch(sender->tls)==epoch);
    int ret=ptls_handle_message(sender->tls,&out,offsets,epoch,input,length,&sender->props);
    CHECK(ret==0||ret==PTLS_ERROR_IN_PROGRESS); CHECK(offsets[0]==0&&offsets[4]==out.off);
    for(size_t i=0;i<4;i++) {
        CHECK(offsets[i]<=offsets[i+1]&&offsets[i+1]<=out.off);
        size_t n=offsets[i+1]-offsets[i]; if(!n) continue; CHECK(i!=1&&tail<64);
        flight_t *f=&flights[tail++]; f->receiver=receiver; f->epoch=i; f->length=n; f->bytes=malloc(n); CHECK(f->bytes);
        memcpy(f->bytes,out.base+offsets[i],n);
    }
    ptls_buffer_dispose(&out);
}
static void pump(peer_t *client,peer_t *server)
{
    head=tail=0; process(client,server,0,NULL,0);
    while(head<tail) { flight_t f=flights[head++]; process(f.receiver,f.receiver==client?server:client,f.epoch,f.bytes,f.length); free(f.bytes); }
    CHECK(ptls_handshake_is_complete(client->tls)&&ptls_handshake_is_complete(server->tls));
    CHECK(client->keys==15&&server->keys==15);
    for(int epoch=0;epoch<2;epoch++) for(int enc=0;enc<2;enc++)
        CHECK(memcmp(client->secrets[epoch][enc],server->secrets[epoch][!enc],client->secret_length)==0);
}
static void reject_helper(peer_t *p)
{
    uint8_t small[4096]; memset(small,0xa5,sizeof(small)); ptls_buffer_t out; ptls_buffer_init(&out,small,sizeof(small)); out.off=4;
    CHECK(dotcc_ptls_send_quic_ticket(p->tls,&out)==PTLS_ALERT_UNEXPECTED_MESSAGE);
    CHECK(out.base==small&&out.capacity==sizeof(small)&&out.off==4);
    for(size_t i=0;i<sizeof(small);i++) CHECK(small[i]==0xa5);
    ptls_buffer_dispose(&out);
}
static void release(peer_t *server,peer_t *client,uint8_t nonce[32],size_t *stored_index)
{
    uint8_t small[4096]; ptls_buffer_t out; ptls_buffer_init(&out,small,sizeof(small));
    size_t epoch=ptls_get_read_epoch(server->tls); unsigned keys=server->keys;
    *stored_index=stored_count;
    CHECK(dotcc_ptls_send_quic_ticket(server->tls,&out)==0);
    CHECK(out.off>=49&&out.base[0]==4&&out.base[12]==32); memcpy(nonce,out.base+13,32);
    CHECK(ptls_get_read_epoch(server->tls)==epoch&&server->keys==keys&&ptls_handshake_is_complete(server->tls));
    CHECK(stored_count==*stored_index+1);
    head=tail=0; process(client,server,3,out.base,out.off); CHECK(tail==0);
    ptls_buffer_dispose(&out);
}
static uint64_t be64(const uint8_t *p) { uint64_t n=0; for(int i=0;i<8;i++) n=(n<<8)|p[i]; return n; }
static void dispose(peer_t *p)
{
    ptls_free(p->tls);
    for(size_t i=0;i<p->saved_count;i++) { ptls_clear_memory(p->saved[i],p->saved_length[i]); free(p->saved[i]); }
    ptls_clear_memory(p,sizeof(*p));
}
static void run_case(X509 *certificate,EVP_PKEY *private_key,ptls_cipher_suite_t *suite)
{
    ptls_openssl_sign_certificate_t signer; CHECK(ptls_openssl_init_sign_certificate(&signer,private_key)==0);
    X509_STORE *trust=X509_STORE_new(); CHECK(trust&&X509_STORE_add_cert(trust,certificate)==1);
    ptls_openssl_verify_certificate_t verifier; CHECK(ptls_openssl_init_verify_certificate(&verifier,trust)==0); X509_STORE_free(trust);
    ptls_cipher_suite_t *suites[]={suite,NULL}; ptls_context_t client_context={0},server_context={0};
    client_context.random_bytes=server_context.random_bytes=ptls_openssl_random_bytes;
    client_context.get_time=server_context.get_time=&clock_source;
    client_context.key_exchanges=server_context.key_exchanges=exchanges;
    client_context.cipher_suites=server_context.cipher_suites=suites;
    client_context.update_traffic_key=server_context.update_traffic_key=&traffic_keys;
    client_context.omit_end_of_early_data=server_context.omit_end_of_early_data=1;
    client_context.require_dhe_on_psk=server_context.require_dhe_on_psk=1;
    client_context.verify_certificate=&verifier.super; client_context.save_ticket=&saver;
    server_context.sign_certificate=&signer.super; server_context.on_client_hello=&hello_cb;
    server_context.encrypt_ticket=&protector; server_context.ticket_lifetime=60;
    CHECK(ptls_openssl_load_certificates(&server_context,certificate,NULL)==0);
    peer_t client,server; initialize(&client,&client_context,0,ptls_iovec_init(NULL,0)); initialize(&server,&server_context,1,ptls_iovec_init(NULL,0));
    reject_helper(&server); reject_helper(&client);
    pump(&client,&server); CHECK(!ptls_is_psk_handshake(client.tls)); reject_helper(&client);
    now_ms+=2*3600*1000; /* Existing connection is older than its original ticket lifetime. */
    uint8_t nonces[2][32]; size_t indexes[2], saved_at=client.saved_count;
    release(&server,&client,nonces[0],&indexes[0]); release(&server,&client,nonces[1],&indexes[1]);
    CHECK(memcmp(nonces[0],nonces[1],32)!=0&&client.saved_count==saved_at+2);
    for(int i=0;i<2;i++) { CHECK(store[indexes[i]].length>20+suite->hash->digest_size); CHECK(be64(store[indexes[i]].session+10)==now_ms); }
    CHECK(memcmp(store[indexes[0]].session+20,store[indexes[1]].session+20,suite->hash->digest_size)!=0);
    for(int i=0;i<2;i++) {
        peer_t resumed_client,resumed_server; unsigned creates=dh_creates,exchanged=dh_exchanges;
        initialize(&resumed_client,&client_context,0,ptls_iovec_init(client.saved[saved_at+i],client.saved_length[saved_at+i]));
        initialize(&resumed_server,&server_context,1,ptls_iovec_init(NULL,0)); pump(&resumed_client,&resumed_server);
        CHECK(ptls_is_psk_handshake(resumed_client.tls)&&ptls_is_psk_handshake(resumed_server.tls));
        CHECK(dh_creates>creates&&dh_exchanges>exchanged); /* Both sides did fresh P-256 work. */
        dispose(&resumed_client); dispose(&resumed_server);
    }
    uint8_t small[4096]; memset(small,0xa5,sizeof(small)); ptls_buffer_t failed; ptls_buffer_init(&failed,small,sizeof(small)); failed.off=4;
    fail_ticket=1; CHECK(dotcc_ptls_send_quic_ticket(server.tls,&failed)==PTLS_ERROR_NO_MEMORY); fail_ticket=0;
    CHECK(failed.off==4&&memcmp(small,"\xa5\xa5\xa5\xa5",4)==0); ptls_buffer_dispose(&failed);
    dispose(&client); dispose(&server);
    for(size_t i=0;i<server_context.certificates.count;i++) free(server_context.certificates.list[i].base);
    free(server_context.certificates.list); ptls_openssl_dispose_sign_certificate(&signer); ptls_openssl_dispose_verify_certificate(&verifier);
    for(size_t i=0;i<stored_count;i++) { ptls_clear_memory(store[i].session,store[i].length); free(store[i].session); }
    memset(store,0,sizeof(store)); stored_count=0;
}
int main(int argc,char **argv)
{
    CHECK(argc==5); alpn=ptls_iovec_init("helper",6); now_ms=(uint64_t)time(NULL)*1000;
    for(int kind=0;kind<2;kind++) {
        FILE *file=fopen(argv[1+kind*2],"r"); CHECK(file); X509 *certificate=PEM_read_X509(file,NULL,NULL,NULL); fclose(file); CHECK(certificate);
        file=fopen(argv[2+kind*2],"r"); CHECK(file); EVP_PKEY *key=PEM_read_PrivateKey(file,NULL,NULL,NULL); fclose(file); CHECK(key);
        run_case(certificate,key,&ptls_openssl_aes128gcmsha256); run_case(certificate,key,&ptls_openssl_aes256gcmsha384);
        X509_free(certificate); EVP_PKEY_free(key);
    }
    puts("PASS native QUIC ticket helper: 4 cipher/certificate cases, delayed fresh tickets, distinct PSKs, fresh-DHE resumption, rejection and rollback");
    return 0;
}
