/* Independent deterministic workload, executed through native and translated CUI. */
#include <cui/cui.h>
#include <cui/cui_render.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int main(void) {
    int failed = 0;
    cui_sw_renderer *sw = cui_sw_create();
    cui_sw_set_threads(sw, 1);
    cui_ctx *ui = cui_create(&(cui_ctx_desc){.renderer=cui_sw_iface(sw)});
    cui_set_reduced_motion(ui, true);
    cui_set_viewport(ui, 320, 200, 1);
    for (int theme=0;theme<2;theme++) {
        cui_set_theme(ui, theme ? CUI_THEME_DARK : CUI_THEME_LIGHT);
        for(int f=0;f<3;f++) {
            cui_begin_frame(ui, 1.0f/60);
            cui_column(ui, .width=CUI_FILL, .height=CUI_FILL, .pad=12, .gap=8, .bg=CUI_COLOR_BACKGROUND) {
                cui_text(ui,"Translated CUI pixels");
                cui_button(ui,"Save",.variant=CUI_BUTTON_PRIMARY);
                cui_progress(ui,0.625f,.label="Storage",.show_value=true);
                cui_draw_rect(ui,(cui_rect){180,100,50,40},CUI_CORNERS(7),CUI_RGB(0x4070c0));
            }
            cui_end_frame(ui);
        }
        unsigned char rgba[320*200*4], bgra[320*200*4];
        const cui_draw_data *dd=cui_get_draw_data(ui);
        cui_sw_render(sw,dd,rgba,320,200,320*4,CUI_SW_RGBA);
        cui_sw_render(sw,dd,bgra,320,200,320*4,CUI_SW_BGRA);
        for(int i=0;i<320*200*4;i+=4)
            if(rgba[i]!=bgra[i+2]||rgba[i+1]!=bgra[i+1]||rgba[i+2]!=bgra[i]||rgba[i+3]!=bgra[i+3]) failed++;
        char name[64];snprintf(name,sizeof name,"pixels-%d.rgba",theme);
        FILE *f=fopen(name,"wb"); if(!f||fwrite(rgba,1,sizeof rgba,f)!=sizeof rgba) return 2; fclose(f);
        printf("theme=%d vertices=%d indices=%d commands=%d bytes=%d\n",theme,dd->vertex_count,dd->index_count,dd->command_count,(int)sizeof rgba);
    }
    cui_destroy(ui);cui_sw_destroy(sw);
    printf("128000 checks, %d failed\n",failed);
    return failed!=0;
}
