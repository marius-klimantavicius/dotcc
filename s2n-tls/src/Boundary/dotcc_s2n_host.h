#pragma once
/* Standard execution service missing from the synthetic stdlib declaration
 * surface. The callback uses the translated managed C calling convention. */
int atexit(void (*function)(void));
