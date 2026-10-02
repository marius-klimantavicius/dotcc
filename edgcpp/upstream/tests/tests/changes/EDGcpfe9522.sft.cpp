//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: __ptr64 on 64-bit targets
// 2/18/09  [EDGcpfe/9522]
//
// Microsoft compatibility: __ptr64 on 64-bit targets
//
// Microsoft compilers appear to ignore the __ptr64 modifier for 64-bit targets.
// Although the front end still records __ptr64 on 64-bit targets, it now ignores
// that modifier for type-equivalence purposes.
//
// Note that the target is considered to be "64-bit" if the target settings for
// size_t correspond to a 64-bit type.  Also note that there is no similar
// behavior for __ptr32 on 32-bit platforms.
extern int *p;
int *__ptr64 p;  // Now accepted on 64-bit platforms.
