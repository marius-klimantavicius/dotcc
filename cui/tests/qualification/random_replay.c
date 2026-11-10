/* Test input provider only. Values are recorded from actual emitted Libc.rand.
 * Queues keyed by seed and source call site preserve mutation/drag inputs when
 * native C chooses a different permitted operand evaluation order.
 */
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
typedef struct record { uint32_t seed; int line,site,value,next; } record;
typedef struct queue { uint32_t seed; int line,site,head,tail,used; } queue;
static record *records;
static queue queues[64];
static int count,nqueues,consumed;
static uint32_t seed=1;
static void done(void) {
    if(consumed!=count){fprintf(stderr,"random input consumption %d/%d\n",consumed,count);_Exit(3);}
    printf("RANDOM_INPUTS %d records, %d call-site queues, all consumed\n",count,nqueues);
}
static void load(void) {
    const char *path=getenv("CUI_QUALIFICATION_RANDOM_TRACE");
    FILE *f=path?fopen(path,"rb"):NULL;if(!f)exit(3);
    fseek(f,0,SEEK_END);long bytes=ftell(f);rewind(f);
    if(bytes<16||bytes%16)exit(3);count=(int)(bytes/16);
    records=calloc((size_t)count,sizeof(*records));if(!records)exit(3);
    for(int i=0;i<count;i++) {
        record *r=&records[i];if(fread(r,4,4,f)!=4)exit(3);r->next=-1;
        int q=0;for(;q<nqueues;q++)if(queues[q].seed==r->seed&&queues[q].line==r->line&&queues[q].site==r->site)break;
        if(q==nqueues){if(nqueues==64)exit(3);queues[q]=(queue){r->seed,r->line,r->site,-1,-1,0};nqueues++;}
        if(queues[q].tail<0)queues[q].head=i;else records[queues[q].tail].next=i;
        queues[q].tail=i;
    }
    fclose(f);atexit(done);
}
void cui_qualification_srand(unsigned value){seed=value;}
int cui_qualification_rand(int line,int site) {
    if(!records)load();
    for(int q=0;q<nqueues;q++)if(queues[q].seed==seed&&queues[q].line==line&&queues[q].site==site){
        int k=queues[q].head;if(k<0)break;
        queues[q].head=records[k].next;queues[q].used++;consumed++;return records[k].value;
    }
    fprintf(stderr,"unexpected random request seed=%u line=%d site=%d\n",seed,line,site);exit(3);
}
