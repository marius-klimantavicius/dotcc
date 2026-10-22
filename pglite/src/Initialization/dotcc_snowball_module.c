/* Forward only to original PostgreSQL Snowball exports. No replacement
 * dictionary, semantic processing or initializer is implemented here. */
#include "postgres.h"
#include "fmgr.h"
#include "dotcc_static_modules.h"

extern const Pg_magic_struct *dpg_snowball_magic(void);
extern Datum dsnowball_init(PG_FUNCTION_ARGS);
extern Datum dsnowball_lexize(PG_FUNCTION_ARGS);
extern const Pg_finfo_record *pg_finfo_dsnowball_init(void);
extern const Pg_finfo_record *pg_finfo_dsnowball_lexize(void);

static const dpg_static_symbol symbols[] = {
    {"Pg_magic_func", (void *) dpg_snowball_magic},
    {"dsnowball_init", (void *) dsnowball_init},
    {"dsnowball_lexize", (void *) dsnowball_lexize},
    {"pg_finfo_dsnowball_init", (void *) pg_finfo_dsnowball_init},
    {"pg_finfo_dsnowball_lexize", (void *) pg_finfo_dsnowball_lexize}
};
static const dpg_static_module module = { sizeof(symbols) / sizeof(symbols[0]), symbols };

const dpg_static_module *dpg_snowball_module(void) { return &module; }
