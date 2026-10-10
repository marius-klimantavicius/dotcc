#include "duckdb_deferred_routine_abi.h"
// This TU knows every source field/signature, but has no result-value ABI yet.
int inspect_callbacks(DeferredCallbacks *callbacks) {
    return callbacks->read(13);
}
