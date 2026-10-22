/* Test-only calls into a real typed exit override; no engine workloads. */
#include <stdlib.h>
int dpg_exit_request(int status) { exit(status); return 42; }
static int value;
int dpg_exit_next(void) { return ++value; }
