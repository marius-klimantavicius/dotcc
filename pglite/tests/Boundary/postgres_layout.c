/* Actual selected PostgreSQL types, not hand-mirrored C/managed structs. */
#include "postgres.h"
#include "access/xlogdefs.h"
#include "datatype/timestamp.h"
#include "lib/stringinfo.h"
#include "storage/itemptr.h"
#include "storage/block.h"

/* Real declared pointer storage for emitted, otherwise unused, upstream inline
 * MemoryContextSwitchTo. No allocator/memory-context operation is implemented.
 */
MemoryContext CurrentMemoryContext = NULL;

/* Keep this probe callable in an emitted library when its header closure works. */
size_t dpg_postgres_layout(unsigned int field) {
    switch (field) {
        case 0: return sizeof(void *);
        case 1: return sizeof(Datum);
        case 2: return sizeof(Oid);
        case 3: return sizeof(Size);
        case 4: return sizeof(TimestampTz);
        case 5: return sizeof(StringInfoData);
        case 6: return offsetof(StringInfoData, data);
        case 7: return offsetof(StringInfoData, len);
        case 8: return offsetof(StringInfoData, maxlen);
        case 9: return offsetof(StringInfoData, cursor);
        case 10: return sizeof(ItemPointerData);
        case 11: return sizeof(BlockIdData);
        case 12: return sizeof(XLogRecPtr);
        default: return 0;
    }
}
#ifdef DPG_NATIVE_MAIN
#include <stdio.h>
/* The reporting harness uses libc, not PostgreSQL's printf portability layer. */
#undef printf
int main(void) {
    const char *names[] = {"pointer", "Datum", "Oid", "Size", "TimestampTz", "StringInfoData",
                          "string_data", "string_len", "string_maxlen", "string_cursor",
                          "ItemPointerData", "BlockIdData", "XLogRecPtr"};
    puts("{");
    for (unsigned int i = 0; i < 13; ++i)
        printf("\"%s\":%zu%s\n", names[i], dpg_postgres_layout(i), i == 12 ? "" : ",");
    puts("}");
    return 0;
}
#endif
