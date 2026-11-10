#define GLFW_INCLUDE_NONE
#include <GLFW/glfw3.h>
void cui_host_probe_declarations(void) {
    (void)glfwCreateStandardCursor(0);
    (void)glfwCreateWindow(0, 0, 0, 0, 0);
    (void)glfwDestroyCursor(0);
    (void)glfwDestroyWindow(0);
    (void)glfwGetClipboardString(0);
    (void)glfwGetCursorPos(0, 0, 0);
    (void)glfwGetFramebufferSize(0, 0, 0);
    (void)glfwGetPrimaryMonitor();
    (void)glfwGetProcAddress(0);
    (void)glfwGetTime();
    (void)glfwGetVideoMode(0);
    (void)glfwGetWindowContentScale(0, 0, 0);
    (void)glfwGetWindowMonitor(0);
    (void)glfwGetWindowUserPointer(0);
    (void)glfwInit();
    (void)glfwMakeContextCurrent(0);
    (void)glfwPollEvents();
    (void)glfwSetCharCallback(0, 0);
    (void)glfwSetClipboardString(0, 0);
    (void)glfwSetCursor(0, 0);
    (void)glfwSetCursorEnterCallback(0, 0);
    (void)glfwSetCursorPosCallback(0, 0);
    (void)glfwSetDropCallback(0, 0);
    (void)glfwSetKeyCallback(0, 0);
    (void)glfwSetMouseButtonCallback(0, 0);
    (void)glfwSetScrollCallback(0, 0);
    (void)glfwSetWindowCloseCallback(0, 0);
    (void)glfwSetWindowFocusCallback(0, 0);
    (void)glfwSetWindowRefreshCallback(0, 0);
    (void)glfwSetWindowSize(0, 0, 0);
    (void)glfwSetWindowSizeLimits(0, 0, 0, 0, 0);
    (void)glfwSetWindowTitle(0, 0);
    (void)glfwSetWindowUserPointer(0, 0);
    (void)glfwShowWindow(0);
    (void)glfwSwapBuffers(0);
    (void)glfwSwapInterval(0);
    (void)glfwTerminate();
    (void)glfwWaitEvents();
    (void)glfwWaitEventsTimeout(0);
    (void)glfwWindowHint(0, 0);
}
