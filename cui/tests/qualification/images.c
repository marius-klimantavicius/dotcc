/* Texture callback observes actual decoded image bytes, before renderer upload. */
#include <cui/cui.h>
#include <stdio.h>
#include <string.h>
static unsigned char decoded[8*8*4];
static int width,height,live,checks,failed;
#define CHECK(x) do{checks++;if(!(x)){failed++;printf("FAIL line %d: %s\n",__LINE__,#x);}}while(0)
static cui_texture create(void *u,int w,int h,cui_texture_format fmt,const void *px) {
    (void)u;
    if(fmt==CUI_TEXTURE_RGBA8&&w<=8&&h<=8){width=w;height=h;memcpy(decoded,px,(size_t)w*h*4);live++;return 42;}
    return 1;
}
static void update(void *u,cui_texture t,int x,int y,int w,int h,const void*p,int s){(void)u;(void)t;(void)x;(void)y;(void)w;(void)h;(void)p;(void)s;}
static void destroy(void *u,cui_texture t){(void)u;if(t==42)live--;}
int main(int argc,char **argv){
    if(argc!=2)return 2;
    cui_ctx *ui=cui_create(&(cui_ctx_desc){.renderer={0,create,update,destroy}});
    const char *names[]={"png","bmp","gif","tga","jpg"};
    const unsigned char expected[]={255,0,0,255,0,255,0,255,0,0,255,255,255,255,255,255};
    for(int i=0;i<5;i++){
        char path[1024];snprintf(path,sizeof path,"%s/colors.%s",argv[1],names[i]);
        int w=-1,h=-1;memset(decoded,0,sizeof decoded);
        cui_texture t=cui_image_load(ui,path,&w,&h);
        CHECK(t==42);CHECK(w==(i==4?8:2)&&h==w);CHECK(width==w&&height==h);
        if(i<4)CHECK(!memcmp(decoded,expected,16));
        else for(int k=0;k<64;k++){CHECK(decoded[k*4]>=77&&decoded[k*4]<=83);CHECK(decoded[k*4+1]>=117&&decoded[k*4+1]<=123);CHECK(decoded[k*4+2]>=157&&decoded[k*4+2]<=163);CHECK(decoded[k*4+3]==255);}
        snprintf(path,sizeof path,"decoded-%s.rgba",names[i]);FILE *f=fopen(path,"wb");if(!f)return 2;fwrite(decoded,1,(size_t)w*h*4,f);fclose(f);
        cui_image_destroy(ui,t);CHECK(live==0);
    }
    char path[1024];snprintf(path,sizeof path,"%s/invalid.bin",argv[1]);
    int w=17,h=19;CHECK(!cui_image_load(ui,path,&w,&h));CHECK(w==17&&h==19);
    snprintf(path,sizeof path,"%s/missing.png",argv[1]);CHECK(!cui_image_load(ui,path,0,0));
    cui_destroy(ui);CHECK(live==0);
    printf("%d checks, %d failed\n",checks,failed);return failed!=0;
}
