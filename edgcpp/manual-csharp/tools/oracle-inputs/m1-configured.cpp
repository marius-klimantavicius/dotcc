#include "m1-configured-quote.h"
#include <m1-configured-system.h>
#ifdef REMOVED
#error explicitly undefined macro remains defined
#endif
static_assert(CONFIGURED_VALUE == 17);
static_assert(quote_value == 18);
static_assert(system_value == 19);
