/* Reduction of postgres src/include/c.h's pg_noreturn extern declaration.
 * Valid C11; no declaration reordering or stripping is applied to upstream.
 */
_Noreturn extern void dpg_never_returns(void);
int dpg_declaration_probe(void) { return 1; }
