/* An upstream-configured unit remains selected even when preprocessing is empty. */
#ifdef ENABLE_UNUSED_FEATURE
int unused_feature(void) { return 99; }
#endif
