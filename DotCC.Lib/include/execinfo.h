#ifndef DOTCC_EXECINFO_H
#define DOTCC_EXECINFO_H
int backtrace(void **frames, int capacity);
char **backtrace_symbols(void *const *frames, int count);
void backtrace_symbols_fd(void *const *frames, int count, int descriptor);
#endif
