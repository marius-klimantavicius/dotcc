/* The same include discovery issue affects upstream's msquic.ver. */
#include "version.inc"
int version(void) { return PROBE_VERSION; }
