/* Compile as one TU to inspect actual private, unchanged SQLite aggregates. */
#include "sqlite3.c"
#include "memory_vfs.c"
#include <stdio.h>
#include <stddef.h>
#define HEADER(T) printf("header %s %lu %lu\n", #T, (unsigned long)sizeof(T), (unsigned long)_Alignof(T))
#define FIELD(T, M) do { T object; printf("field %s %s %lu %ld\n", #T, #M, (unsigned long)offsetof(T, M), (long)((char *)&object.M - (char *)&object)); } while (0)
#define REQUIRED(T, M) do { T object; printf("required %s %s %lu %lu %lu %ld\n", #T, #M, (unsigned long)sizeof(T), (unsigned long)_Alignof(T), (unsigned long)offsetof(T, M), (long)((char *)&object.M - (char *)&object)); } while (0)
/* Pointer elements deliberately exercise generated InlineArray storage. The
** last element must have real storage, distinct from the first element. */
#define POINTER_ARRAY(T, M) do { \
  T object; char first, last; \
  unsigned long count = sizeof(object.M) / sizeof(object.M[0]); \
  object.M[0] = (void *)&first; object.M[count-1] = (void *)&last; \
  printf("array %s %s %lu %lu %lu %lu %lu %ld %ld %d %d\n", \
    #T, #M, (unsigned long)sizeof(T), (unsigned long)_Alignof(T), \
    (unsigned long)sizeof(object.M), (unsigned long)sizeof(object.M[0]), count, \
    (long)((char *)&object.M[0] - (char *)&object), \
    (long)((char *)&object.M[count-1] - (char *)&object), \
    object.M[0] == (void *)&first, object.M[count-1] == (void *)&last); \
} while (0)
int main(void) {
  HEADER(sqlite3_vfs); FIELD(sqlite3_vfs, xOpen); FIELD(sqlite3_vfs, xCurrentTimeInt64);
  HEADER(sqlite3_io_methods); FIELD(sqlite3_io_methods, xRead); FIELD(sqlite3_io_methods, xFetch);
  HEADER(sqlite3_index_info); FIELD(sqlite3_index_info, aConstraint); FIELD(sqlite3_index_info, estimatedCost); FIELD(sqlite3_index_info, colUsed);
  HEADER(Mem); FIELD(Mem, flags); FIELD(Mem, db); FIELD(Mem, xDel);
  HEADER(FKey); FIELD(FKey, nCol); FIELD(FKey, aCol);
  HEADER(ExprList); FIELD(ExprList, a);
  HEADER(struct ExprList_item); FIELD(struct ExprList_item, fg); FIELD(struct ExprList_item, u);
  HEADER(SrcList); FIELD(SrcList, a);
  HEADER(VdbeCursor); FIELD(VdbeCursor, seekHit); FIELD(VdbeCursor, ub); FIELD(VdbeCursor, uc); FIELD(VdbeCursor, aType);
  HEADER(JsonParse); FIELD(JsonParse, nBlob); FIELD(JsonParse, delta); FIELD(JsonParse, aIns);
  POINTER_ARRAY(FuncDefHash, a);
  POINTER_ARRAY(JsonCache, a);
  POINTER_ARRAY(FKey, apTrigger);
  POINTER_ARRAY(MemPage, apOvfl);
  POINTER_ARRAY(BtCursor, apPage);
  POINTER_ARRAY(CellArray, apEnd);
  POINTER_ARRAY(PragmaVtabCursor, azArg);
  POINTER_ARRAY(WhereLoop, aLTermSpace);
#ifdef DOTCC_LAYOUT_REQUESTS
#include "layout_requests.h"
#endif
  return 0;
}
