/* Native GLFW control for the separate managed consumer's desktop workload. */
#include <cui/cui_app.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main(int argc, char **argv) {
    if (argc != 2) return 2;
    cui_app_desc desc = {.title="CUI native control", .width=640, .height=480,
        .hidden=true, .continuous=true, .renderer=CUI_RENDERER_OPENGL,
        .theme=CUI_THEME_DARK, .accent=CUI_ACCENT_BLUE};
    cui_app *app = cui_app_create(&desc);
    if (!app || cui_app_is_software(app) || strcmp(cui_app_backend(app), "glfw")) return 3;
    int frames=0, result=0;
    bool enabled=true;
    float value=0.5f;
    char input[128]={0};
    while (cui_app_frame(app)) {
        cui_ctx *ui=cui_app_ui(app);
        cui_column_begin(ui, NULL);
        cui_label(ui, "Translated CUI: widgets, input and OpenGL");
        cui_button_ex(ui, "Count click", NULL);
        cui_checkbox_ex(ui, "Enabled", &enabled, NULL);
        cui_slider_ex(ui, "Value", &value, 0, 1, NULL);
        cui_input_ex(ui, "message", input, sizeof input, NULL);
        cui_box_end(ui);
        if (++frames==4) {
            size_t size=640*480*4;
            unsigned char *pixels=malloc(size);
            if (!pixels || !cui_app_screenshot(app, pixels, 640, 480)) result=4;
            else {
                int colored=0;
                for (size_t i=4;i<size;i+=4) if (memcmp(pixels+i,pixels,3)) colored++;
                FILE *file=fopen(argv[1],"wb");
                if (!file || fwrite(pixels,1,size,file)!=size) result=5;
                if (file) fclose(file);
                if (colored<100) result=6;
                printf("Native GLFW OpenGL: %d frames, 640x480, %d differing pixels\n",frames,colored);
            }
            free(pixels);
            cui_app_quit(app);
        }
    }
    cui_app_destroy(app);
    return result;
}
