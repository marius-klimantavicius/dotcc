/* Reduced from lib/sha.h: a standalone anonymous enum definition. */
enum { shaSuccess = 0, shaNull, shaInputTooLong, shaStateError, shaBadParam };
int result(void) { return shaSuccess; }
