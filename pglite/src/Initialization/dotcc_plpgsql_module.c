/* Original PostgreSQL exports only. dfmgr still validates the real module
 * magic and invokes the real load-time initializer and language handlers. */
#include "postgres.h"
#include "fmgr.h"
#include "dotcc_static_modules.h"

extern const Pg_magic_struct *dpg_plpgsql_magic(void);
extern void dpg_plpgsql_init(void);
extern Datum plpgsql_call_handler(PG_FUNCTION_ARGS);
extern Datum plpgsql_inline_handler(PG_FUNCTION_ARGS);
extern Datum plpgsql_validator(PG_FUNCTION_ARGS);
extern const Pg_finfo_record *pg_finfo_plpgsql_call_handler(void);
extern const Pg_finfo_record *pg_finfo_plpgsql_inline_handler(void);
extern const Pg_finfo_record *pg_finfo_plpgsql_validator(void);

static const dpg_static_symbol symbols[] = {
    {"Pg_magic_func", (void *) dpg_plpgsql_magic},
    {"_PG_init", (void *) dpg_plpgsql_init},
    {"plpgsql_call_handler", (void *) plpgsql_call_handler},
    {"plpgsql_inline_handler", (void *) plpgsql_inline_handler},
    {"plpgsql_validator", (void *) plpgsql_validator},
    {"pg_finfo_plpgsql_call_handler", (void *) pg_finfo_plpgsql_call_handler},
    {"pg_finfo_plpgsql_inline_handler", (void *) pg_finfo_plpgsql_inline_handler},
    {"pg_finfo_plpgsql_validator", (void *) pg_finfo_plpgsql_validator}
};
static const dpg_static_module module = { sizeof(symbols) / sizeof(symbols[0]), symbols };

const dpg_static_module *dpg_plpgsql_module(void) { return &module; }
