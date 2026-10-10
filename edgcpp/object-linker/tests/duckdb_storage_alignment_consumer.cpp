// Incomplete array declarations retain source type and require element alignment.
extern int *duckdb_alignment_table[];
alignas(32) extern int *duckdb_alignment_overaligned_table[];
extern "C" int duckdb_alignment_read(int **);
int main() {
    if (duckdb_alignment_read(duckdb_alignment_table) != 13) return 1;
    if (duckdb_alignment_read(duckdb_alignment_overaligned_table) != 13) return 2;
    return reinterpret_cast<unsigned long>(duckdb_alignment_overaligned_table) % 32 ? 3 : 0;
}
