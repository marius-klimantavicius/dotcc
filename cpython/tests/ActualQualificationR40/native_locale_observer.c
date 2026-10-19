/* New native CONTROL ONLY. The existing B37 Host executable is unchanged.
 * This constructor models a consumer selecting its process locale before the
 * exact Host PyPreConfig_InitIsolatedConfig/configure_locale=0 boundary. */
#include <locale.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

__attribute__((constructor)) static void observe_consumer_locale(void)
{
    char before[128];
    snprintf(before, sizeof(before), "%s", setlocale(LC_CTYPE, NULL));
    setenv("DOTCC_R40_INITIAL_LOCALE", before, 1);
    if (getenv("DOTCC_R40_SELECT_LOCALE") && strcmp(getenv("DOTCC_R40_SELECT_LOCALE"), "1") == 0)
        if (!setlocale(LC_CTYPE, "C.UTF-8")) _exit(71);
    setenv("DOTCC_R40_SELECTED_LOCALE", setlocale(LC_CTYPE, NULL), 1);
    printf("native consumer BEFORE preinit initial=%s selected=%s\n", before, setlocale(LC_CTYPE, NULL));
}
