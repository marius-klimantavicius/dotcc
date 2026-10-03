#include <stdlib.h>
#include "macro_exports.h"
int macro_export_source_check(void) { return API_INT == 4096 && API_REDEFINED == 9; }

void macro_set_live(int value) { macro_live_value = value; }

static void macro_cleanup(void) { macro_live_value = 0; }
int macro_register_cleanup(void) { return atexit(macro_cleanup); }
#ifdef __cplusplus
struct MacroGuard { ~MacroGuard() { ++macro_live_value; } };
int macro_guarded(int value) { MacroGuard guard; macro_set_live(value); return API_TWICE(value); }
#endif
