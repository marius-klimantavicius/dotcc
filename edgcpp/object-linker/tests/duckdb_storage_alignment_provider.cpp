int duckdb_alignment_number = 13;
int *duckdb_alignment_table[] = { &duckdb_alignment_number };
alignas(32) int *duckdb_alignment_overaligned_table[] = { &duckdb_alignment_number };
extern "C" int duckdb_alignment_read(int **table) { return **table; }
