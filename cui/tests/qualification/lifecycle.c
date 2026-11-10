/* Offscreen lifetimes and the documented first-allocation failure contract. */
#include <cui/cui.h>
#include <cui/cui_render.h>
#include <stdio.h>
#include <stdlib.h>
static int live,checks,failed;
#define CHECK(x) do{checks++;if(!(x)){failed++;printf("FAIL line %d: %s\n",__LINE__,#x);}}while(0)
static void *allocate(size_t n,void *u){(void)u;void *p=malloc(n);if(p)live++;return p;}
static void *resize(void *p,size_t n,void *u){(void)u;if(!p)return allocate(n,u);return realloc(p,n);}
static void release(void *p,void *u){(void)u;if(p){live--;free(p);}}
static void *refuse(size_t n,void *u){(void)n;(void)u;return NULL;}
int main(void){
    cui_destroy(NULL);cui_sw_destroy(NULL);
    cui_ctx *bad=cui_create(&(cui_ctx_desc){.allocator={.alloc=refuse,.realloc=resize,.free=release}});
    CHECK(bad==NULL);
    for(int cycle=0;cycle<8;cycle++){
        cui_sw_renderer *sw=cui_sw_create();CHECK(sw!=NULL);
        cui_ctx *ui=cui_create(&(cui_ctx_desc){.renderer=cui_sw_iface(sw),.allocator={.alloc=allocate,.realloc=resize,.free=release}});CHECK(ui!=NULL);
        cui_set_viewport(ui,200,100,1);
        for(int f=0;f<3;f++){cui_begin_frame(ui,1.0f/60);cui_button(ui,"Lifetime");cui_end_frame(ui);}
        CHECK(cui_get_draw_data(ui)->vertex_count>0);
        cui_destroy(ui);cui_sw_destroy(sw);CHECK(live==0);
    }
    printf("%d checks, %d failed\n",checks,failed);return failed!=0;
}
