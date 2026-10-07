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
      using namespace semantic_storage;
      Database database(argv[1]);
      Properties metadata;
      metadata.set("schema",Value::text("edg-semantic/1"));
      metadata.set("lowered",Value::boolean(false));
      metadata.set("completeForEmission",Value::boolean(false));
      metadata.set("producerVersion",Value::text("native-writer-regression"));
      metadata.set("sourceLanguage",Value::text("C"));
      for(const auto &fact:std::vector<std::pair<std::string,std::uint64_t>>{
          {"standardVersion",2017},{"rootScope",1},{"primarySource",2},{"mainRoutine",0},
          {"pointerBytes",8},{"intBytes",4},{"longBytes",8},{"longDoubleBytes",16}})
        metadata.set(fact.first,Value::unsigned_integer(fact.second));
      metadata.set("littleEndian",Value::boolean(true));
      metadata.set("plainCharSigned",Value::boolean(true));
      metadata.set("configuration",Value::object({{"DO_IL_LOWERING",Value::signed_integer(0)}}));
      database.metadata(metadata);

      NodeRecord scope(1,"scope");
      scope.set("form",Value::text("sck_file"));
      for(const auto &fact:std::vector<std::pair<std::string,std::uint64_t>>{
          {"next",0},{"parent",0},{"children",0},{"variables",0},{"types",5},{"routines",0}})
        scope.set(fact.first,Value::unsigned_integer(fact.second));
      scope.edge(5,"type"); database.node(scope);
      NodeRecord source(2,"source-file");
      for(const char *key:{"path","fullPath","nameAsWritten"})
        source.set(key,Value::text("native-writer-fixture.c"));
      source.set("next",Value::unsigned_integer(0));
      source.set("firstChild",Value::unsigned_integer(0));
      source.begin_edges(); database.node(source);

      const std::uint64_t max_signed=static_cast<std::uint64_t>(std::numeric_limits<std::int64_t>::max());
      const std::uint64_t max_unsigned=std::numeric_limits<std::uint64_t>::max();
      const auto supplement=Value::object({{"nested",Value::array({
          Value::boolean(false),Value::null(),Value::unsigned_integer(max_unsigned)})}});
      const auto constant=[](std::uint64_t id,const char *digits) {
        NodeRecord n(id,"constant"); n.set("form",Value::text("ck_integer"));
        n.set("type",Value::unsigned_integer(5)); n.set("integerValue",Value::text(digits)); return n;
      };
      NodeRecord n3=constant(3,"18446744073709551615");
      n3.set("probeUnsigned",Value::unsigned_integer(max_unsigned));
      n3.set("probeSigned",Value::signed_integer(std::numeric_limits<std::int64_t>::min()));
      n3.set("probeFlag",Value::boolean(true));
      n3.set("probeNull",Value::null());
      // Native bytes are Latin1; a separately supplied UTF-8 value must pool
      // identically after conversion, including the embedded NUL.
      n3.set("payload",Value::latin1("same\0\xe9",6));
      n3.set("supplement",supplement);
      n3.set("probeFlag",Value::boolean(false)); // Last value, first field order.
      n3.set("signedBoundary",Value::signed_integer(std::numeric_limits<std::int64_t>::max()));
      n3.set("unsignedBoundary",Value::unsigned_integer(max_signed));
      n3.set("wideBoundary",Value::unsigned_integer(max_signed+1));
      n3.begin_edges(); database.node(n3);
      NodeRecord n4=constant(4,"0");
      n4.set("payload",Value::text(std::string("same\0\xc3\xa9",7))); database.node(n4);

      NodeRecord type(5,"type");
      type.set("form",Value::text("tk_integer"));
      type.set("name",Value::text("unsigned long long"));
      type.set("size",Value::unsigned_integer(8)); type.set("alignment",Value::unsigned_integer(8));
      type.set("integerKind",Value::text("ik_unsigned_long_long"));
      for(const char *key:{"signed","enum","bool"}) type.set(key,Value::boolean(false));
      type.set("canonicalType",Value::unsigned_integer(5)); type.set("parentScope",Value::unsigned_integer(1));
      type.edge(1,"scope"); database.node(type);

      NodeRecord expr(6,"expr-node");
      expr.set("form",Value::text("constant")); expr.set("type",Value::unsigned_integer(5));
      expr.set("constant",Value::unsigned_integer(3)); expr.set("valueCategory",Value::text("prvalue"));
      const SourceLocation escaped_location={10,1,1,2,
          latin1_to_utf8("p\0\xe9\n\"\\",6),"native-writer-fixture.c"};
      const SourcePosition position={escaped_location,escaped_location,0};
      expr.set("position",position_value(position));
      expr.set("range",range_value({true,position,position}));
      expr.edge(3,"constant"); expr.edge(5,"type"); database.node(expr);
      // Add a new column after earlier rows, then reuse an already-prepared
      // insertion shape to verify schema changes do not disturb old bindings.
      NodeRecord n7=constant(7,"1");
      n7.set("probeFlag",Value::boolean(true)); n7.set("lateField",Value::unsigned_integer(29));
      n7.set("payload",Value::latin1("same\0\xe9",6)); n7.set("supplement",supplement);
      // Signed and unsigned inputs share one integer column kind.
      n7.set("unsignedBoundary",Value::signed_integer(42)); database.node(n7);
      NodeRecord n8=constant(8,"0"); n8.set("payload",Value::latin1("same\0\xe9",6)); database.node(n8);
      NodeRecord macro(9,"macro"); macro.set("name",Value::text("CHECK"));
      macro.set("next",Value::unsigned_integer(0)); macro.set("text",Value::text("x"));
      macro.set("objectLike",Value::boolean(false)); macro.begin_edges(); database.node(macro);
      database.macro({1,0,9,
          {10,1,1,2,"native-writer-fixture.c","native-writer-fixture.c"},
          {10,6,1,2,"native-writer-fixture.c","native-writer-fixture.c"},latin1_to_utf8("same\0\xe9",6)});
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
    {
      Query query(reader.db,"SELECT signedBoundary,unsignedBoundary,wideBoundary FROM node_constant WHERE node_id=3");
      require(query.row() && query.type(0)==SQLITE_INTEGER && query.type(1)==SQLITE_INTEGER &&
              query.integer(0)==std::numeric_limits<std::int64_t>::max() && query.integer(1)==query.integer(0),
              "INT64_MAX signed/unsigned typed binding changed");
      require(query.type(2)==SQLITE_BLOB && query.bytes(2)=="9223372036854775808",
              "INT64_MAX+1 did not use exact decimal BLOB storage");
    }
    equals(reader.db,"SELECT unsignedBoundary FROM node_constant WHERE node_id=7",42,
           "Signed/unsigned integer values acquired incompatible field kinds");
    equals(reader.db,"SELECT ordinal FROM node_fields f JOIN node_kinds k USING(kind_id) WHERE k.name='constant' AND f.name='probeFlag'",4,
           "Duplicate field overwrite changed discovery order");
    {
      const std::string expected_location=R"JSON({"sequence":10,"column":1,"line":1,"source":2,"presumedFile":"p\u0000\u00e9\u000a\"\\","physicalFile":"native-writer-fixture.c"})JSON";
      const std::string expected_position="{\"expansion\":"+expected_location+",\"spelling\":"+expected_location+",\"macroContext\":0}";
      Query query(reader.db,"SELECT p.payload,r.payload FROM node_expr_node n JOIN literals p ON p.literal_id=n.position JOIN literals r ON r.literal_id=n.range WHERE n.node_id=6");
      require(query.row() && query.bytes(0)==expected_position,
              "Typed source position escaped non-ASCII/control text differently");
      require(query.bytes(1)=="{\"available\":true,\"start\":"+expected_position+",\"end\":"+expected_position+"}",
              "Typed source range structure changed");
      require(semantic_storage::json_value(semantic_storage::Value::object({
          {"first",semantic_storage::Value::signed_integer(1)},
          {"second",semantic_storage::Value::boolean(false)},
          {"first",semantic_storage::Value::signed_integer(2)}}))=="{\"first\":2,\"second\":false}",
          "Structured property overwrite changed value or member order");
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
