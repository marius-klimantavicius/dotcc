// Standalone regression of the same native writer included by the EDG backend.
// Link against the pinned host SQLite library from the isolated native build.
#include "semantic_sqlite.h"
#include <iostream>
#include <stdexcept>
#include <string>

namespace {
void require(bool condition, const char *message) {
  if (!condition) throw std::runtime_error(message);
}
struct ReadDatabase {
  sqlite3 *db = nullptr;
  explicit ReadDatabase(const char *path) {
    require(sqlite3_open_v2(path, &db, SQLITE_OPEN_READONLY, nullptr) == SQLITE_OK,
            "Cannot reopen native database");
  }
  ~ReadDatabase() { if (db) sqlite3_close(db); }
};
struct Query {
  sqlite3_stmt *statement = nullptr;
  Query(sqlite3 *db, const char *sql) {
    require(sqlite3_prepare_v2(db, sql, -1, &statement, nullptr) == SQLITE_OK,
            "Cannot prepare native writer assertion");
  }
  ~Query() { sqlite3_finalize(statement); }
  bool row() { return sqlite3_step(statement) == SQLITE_ROW; }
  sqlite3_int64 integer(int column = 0) { return sqlite3_column_int64(statement, column); }
  int type(int column = 0) { return sqlite3_column_type(statement, column); }
  std::string bytes(int column = 0) {
    const char *p = static_cast<const char*>(sqlite3_column_blob(statement, column));
    return p ? std::string(p, static_cast<size_t>(sqlite3_column_bytes(statement, column))) : std::string();
  }
};
void equals(sqlite3 *db, const char *sql, sqlite3_int64 expected, const char *message) {
  Query query(db, sql); require(query.row() && query.integer() == expected, message);
  require(!query.row(), "Scalar assertion returned multiple rows");
}
} // namespace

int main(int argc, char **argv) {
  try {
    require(argc == 2, "Usage: native-writer FRESH.sqlite");
    {
      semantic_storage::Database database(argv[1]);
      database.metadata(R"JSON({"schema":"edg-semantic/1","lowered":false,"completeForEmission":false,"producerVersion":"native-writer-regression","sourceLanguage":"C","standardVersion":2017,"rootScope":1,"primarySource":2,"mainRoutine":0,"pointerBytes":8,"intBytes":4,"longBytes":8,"longDoubleBytes":16,"littleEndian":true,"plainCharSigned":true,"configuration":{"DO_IL_LOWERING":0}})JSON");
      database.node(R"JSON({"id":1,"kind":"scope","form":"sck_file","next":0,"parent":0,"children":0,"variables":0,"types":5,"routines":0,"nativeEdges":[{"target":5,"kind":"type"}]})JSON");
      database.node(R"JSON({"id":2,"kind":"source-file","path":"native-writer-fixture.c","fullPath":"native-writer-fixture.c","nameAsWritten":"native-writer-fixture.c","next":0,"firstChild":0,"nativeEdges":[]})JSON");
      database.node(R"JSON({"id":3,"kind":"constant","form":"ck_integer","type":5,"integerValue":"18446744073709551615","probeUnsigned":18446744073709551615,"probeSigned":-9223372036854775808,"probeFlag":false,"probeNull":null,"payload":"same\u0000\u00e9","supplement":{"nested":[false,null,18446744073709551615]},"nativeEdges":[]})JSON");
      database.node(R"JSON({"id":4,"kind":"constant","form":"ck_integer","type":5,"integerValue":"0","payload":"same\u0000\u00e9"})JSON");
      database.node(R"JSON({"id":5,"kind":"type","form":"tk_integer","name":"unsigned long long","size":8,"alignment":8,"integerKind":"ik_unsigned_long_long","signed":false,"enum":false,"bool":false,"canonicalType":5,"parentScope":1,"nativeEdges":[{"target":1,"kind":"scope"}]})JSON");
      database.node(R"JSON({"id":6,"kind":"expr-node","form":"constant","type":5,"constant":3,"valueCategory":"prvalue","nativeEdges":[{"target":3,"kind":"constant"},{"target":5,"kind":"type"}]})JSON");
      // Add a new column after earlier rows, then reuse an already-prepared
      // insertion shape to verify schema changes do not disturb old bindings.
      database.node(R"JSON({"id":7,"kind":"constant","form":"ck_integer","type":5,"integerValue":"1","probeFlag":true,"lateField":29,"payload":"same\u0000\u00e9","supplement":{"nested":[false,null,18446744073709551615]}})JSON");
      database.node(R"JSON({"id":8,"kind":"constant","form":"ck_integer","type":5,"integerValue":"0","payload":"same\u0000\u00e9"})JSON");
      database.node(R"JSON({"id":9,"kind":"macro","name":"CHECK","next":0,"text":"x","objectLike":false,"nativeEdges":[]})JSON");
      database.macro(R"JSON({"index":1,"parentIndex":0,"definition":9,"start":{"sequence":10,"column":1,"line":1,"source":2,"presumedFile":"native-writer-fixture.c","physicalFile":"native-writer-fixture.c"},"end":{"sequence":10,"column":6,"line":1,"source":2,"presumedFile":"native-writer-fixture.c","physicalFile":"native-writer-fixture.c"},"arguments":"same\u0000\u00e9"})JSON");
      database.diagnostic(3, "retained diagnostic");
      database.diagnostic(6, "retained diagnostic");
      database.commit();
    }
    ReadDatabase reader(argv[1]);
    equals(reader.db, "SELECT count(*) FROM nodes", 9, "Node identities were lost");
    equals(reader.db, "SELECT count(*) FROM units WHERE unit_id=1", 1, "Unit identity missing");
    {
      Query query(reader.db, "SELECT probeUnsigned,probeSigned,probeFlag,probeNull FROM node_constant WHERE node_id=3");
      require(query.row(), "Constant record missing");
      require(query.type(0) == SQLITE_BLOB && query.bytes(0) == "18446744073709551615", "UINT64_MAX was rounded or changed");
      require(query.type(1) == SQLITE_INTEGER && query.integer(1) == (-9223372036854775807LL-1), "INT64_MIN changed");
      require(query.type(2) == SQLITE_INTEGER && query.integer(2) == 0, "False boolean changed");
      require(query.type(3) == SQLITE_INTEGER && query.integer(3) == 0, "Explicit null marker missing");
    }
    {
      Query query(reader.db, "SELECT probeFlag,probeNull,lateField FROM node_constant WHERE node_id=4");
      require(query.row() && query.type(0) == SQLITE_NULL && query.type(1) == SQLITE_NULL && query.type(2) == SQLITE_NULL,
              "Absent fields were populated by reused bindings");
    }
    equals(reader.db, "SELECT count(*) FROM node_constant WHERE payload=(SELECT payload FROM node_constant WHERE node_id=3)", 4, "Equal literals were not pooled across distinct nodes");
    equals(reader.db, "SELECT count(DISTINCT node_id) FROM node_constant", 4, "Literal pooling merged semantic nodes");
    equals(reader.db, "SELECT count(DISTINCT supplement) FROM node_constant WHERE supplement IS NOT NULL", 1, "Structured payload pooling changed");
    {
      Query query(reader.db, "SELECT l.encoding,l.payload FROM node_constant n JOIN literals l ON l.literal_id=n.payload WHERE n.node_id=3");
      require(query.row() && query.bytes(0) == "text" && query.bytes(1) == std::string("same\0\xc3\xa9", 7),
              "Embedded NUL or decoded Unicode bytes changed");
    }
    {
      Query query(reader.db, "SELECT value_kind FROM node_fields f JOIN node_kinds k USING(kind_id) WHERE k.name='constant' AND f.name='probeNull'");
      require(query.row() && query.bytes() == "null", "Explicit null field category missing");
    }
    equals(reader.db, "SELECT edge_count FROM nodes WHERE node_id=3", 0, "Empty native edge list changed");
    {
      Query query(reader.db, "SELECT edge_count FROM nodes WHERE node_id=4");
      require(query.row() && query.type() == SQLITE_NULL, "Absent native edge list became empty");
    }
    equals(reader.db, "SELECT lateField FROM node_constant WHERE node_id=7", 29, "Late typed field missing");
    equals(reader.db, "SELECT count(*) FROM native_edges e LEFT JOIN nodes n ON n.unit_id=e.unit_id AND n.node_id=e.target_id WHERE n.node_id IS NULL", 0, "Native reference closure failed");
    equals(reader.db, "SELECT count(*) FROM native_edges e JOIN nodes n ON n.unit_id=e.unit_id AND n.node_id=e.target_id WHERE e.target_kind_id<>n.kind_id", 0, "Edge target kind changed");
    equals(reader.db, "SELECT count(*) FROM macros WHERE invocation_id=1 AND parent_index=0 AND definition_id=9 AND arguments_literal_id=(SELECT payload FROM node_constant WHERE node_id=3)", 1, "Macro facts or argument pooling changed");
    equals(reader.db, "SELECT count(*) FROM diagnostics", 2, "Diagnostics were dropped");
    equals(reader.db, "SELECT count(DISTINCT message_literal_id) FROM diagnostics", 1, "Diagnostic payloads were not pooled");
    equals(reader.db, "SELECT count(*) FROM package_info", 0, "Native graph falsely claims a complete package");
    equals(reader.db, "SELECT count(*) FROM source_files", 0, "Native writer fabricated source snapshots");
    {
      Query query(reader.db, "PRAGMA integrity_check");
      require(query.row() && query.bytes() == "ok", "SQLite integrity check failed");
    }
    {
      // Deliberately place distinct payloads in one hash bucket. Exercise the
      // exact production candidate scanner without relying on finding an
      // accidental 64-bit FNV collision or adding a test-only hash override.
      Query query(reader.db, "WITH candidates(literal_id,payload_hash,payload) AS (VALUES(1,7,X'6162'),(2,7,X'6364'),(3,9,X'6566')) SELECT literal_id,payload FROM candidates WHERE payload_hash=7 ORDER BY literal_id");
      require(semantic_storage::find_exact_literal(query.statement, "cd") == 2,
              "Hash collision merged unequal payloads");
      sqlite3_reset(query.statement);
      require(semantic_storage::find_exact_literal(query.statement, "ef") == 0,
              "Hash bucket returned an unequal payload");
      sqlite3_reset(query.statement);
      require(semantic_storage::find_exact_literal(query.statement, "ab") == 1,
              "Exact payload in a collision bucket was not found");
    }
    bool rejected = false;
    try { semantic_storage::Database overwrite(argv[1]); }
    catch (const std::runtime_error&) { rejected = true; }
    require(rejected, "Native writer allowed overwriting an existing database");
    std::cout << "PASS native semantic SQLite writer: exact integers, typed fields, literals, hash collisions, identities, edges, macros, diagnostics\n";
    return 0;
  } catch (const std::exception &error) {
    std::cerr << "native-writer: " << error.what() << '\n';
    return 1;
  }
}
