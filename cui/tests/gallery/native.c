/* Original upstream gallery, linked to a native CUI GLFW control. */
#include <cui/cui_app.h>
#include "cui_internal.h"
#include "gallery_ui.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* Same explicit render boundary as DesktopApplication.Render(). */
void cui__app_render(cui_app *app);
int native_input(void);

int main(int argc, char **argv) {
    setvbuf(stdout, NULL, _IOLBF, 0);
    if (argc == 2 && !strcmp(argv[1], "--input")) return native_input();
    if (argc != 2) return 2;
    cui_app_desc desc = {.title="Native CUI gallery qualification", .width=1240,
        .height=820, .continuous=true, .renderer=CUI_RENDERER_OPENGL,
        .theme=CUI_THEME_LIGHT, .accent=CUI_ACCENT_NEUTRAL};
    cui_app *app = cui_app_create(&desc);
    if (!app || cui_app_is_software(app) || strcmp(cui_app_backend(app), "glfw")) return 3;
    cui_ctx *ui = cui_app_ui(app);
    cui_set_theme(ui, CUI_THEME_LIGHT);
    cui_set_accent(ui, CUI_ACCENT_NEUTRAL);
    if (ui->theme_t != 1) { cui_app_destroy(app); return 10; }
    gallery g;
    gallery_init(&g);
    int total = gallery_page_total(), result = 0;
    if (total != 27) result = 4;
    unsigned char *pixels = malloc(1240 * 820 * 4);
    if (!pixels) result = 5;
    for (int page = 0; !result && page < total; page++) {
        g.page = page;
        cui_scroll_to(ui, "main", 0);
        for (int frame = 0; frame < 4; frame++) {
            if (!cui_app_frame(app)) { result = 6; break; }
            /* Pin only the capture clock, preserving translated widget code. */
            ui->time = 10;
            ui->dt = 1.0f / 60;
            gallery_frame(ui, &g);
            cui__app_render(app);
        }
        if (result) break;
        if (!cui_app_screenshot(app, pixels, 1240, 820)) { result = 7; break; }
        unsigned text_hash = 2166136261u;
        int main_text = 0, different = 0;
        char path[4096];
        snprintf(path, sizeof path, "%s/page-%02d.text", argv[1], page);
        FILE *labels = fopen(path, "wb");
        if (!labels) { result = 9; break; }
        for (int i = 0; i < ui->node_count; i++) {
            cui_node *n = &ui->nodes[i];
            if (n->text && n->rect.x >= 260) {
                main_text++;
                fwrite(n->text, 1, n->text_len, labels);
                fputc('\n', labels);
                for (int j = 0; j < n->text_len; j++) text_hash = (text_hash ^ (unsigned char)n->text[j]) * 16777619u;
                text_hash = (text_hash ^ 0xffu) * 16777619u;
            }
        }
        fclose(labels);
        for (int y = 0; y < 820; y++)
            for (int x = 261; x < 1240; x++)
                if (memcmp(pixels + (y * 1240 + x) * 4, pixels + (y * 1240 + 260) * 4, 3)) different++;
        if (main_text < 3 || different < 1000) { result = 8; break; }
        snprintf(path, sizeof path, "%s/page-%02d.rgba", argv[1], page);
        FILE *file = fopen(path, "wb");
        if (!file || fwrite(pixels, 4, 1240 * 820, file) != 1240 * 820) result = 9;
        if (file) fclose(file);
        printf("PAGE\t%d\t%s\t%d\t%d\t%u\t%d\n", page, gallery_page_name(page), ui->node_count, main_text, text_hash, different);
        fflush(stdout);
    }
    free(pixels);
    cui_app_quit(app);
    cui_app_destroy(app);
    if (!result) puts("PASS native gallery: 27 rendered pages, main-content text and nonblank GL readback, cleanup");
    return result;
}
