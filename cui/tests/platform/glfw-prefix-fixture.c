/* Only the actual init/allocation-failure prefix is selected. Success past
 * that prefix aborts: this cannot pass as window-initialization evidence. */
#include <GLFW/glfw3.h>
static int glfw_init_ok, glfw_inits, glfw_terminates, glfw_allocations;
int glfwInit(void) { glfw_inits++; return glfw_init_ok; }
void glfwTerminate(void) { glfw_terminates++; }
static void *glfw_fail_calloc(size_t count, size_t size) { (void)count; (void)size; glfw_allocations++; return NULL; }
#define calloc glfw_fail_calloc
#define cui__plat_init platform_glfw_init_prefix
#include "platform-glfw-prefix-selected.c"
#undef cui__plat_init
#undef calloc
int platform_glfw_allocation_case(int init_ok) {
    glfw_init_ok = init_ok; glfw_inits = glfw_terminates = glfw_allocations = 0;
    cui_app app; memset(&app, 0, sizeof app);
    CHECK(!platform_glfw_init_prefix(&app));
    CHECK(app.plat == NULL && glfw_inits == 1);
    CHECK(glfw_terminates == init_ok && glfw_allocations == init_ok);
    return 0;
}
