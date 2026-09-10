/* Shared native/translated FTS5 contract. All registration is explicit. */
#include "memory_vfs.h"
#include <stdio.h>
#include <string.h>

#define REQUIRE(x) do { if (!(x)) { printf("FAIL FTS5 line %d: %s: %s\n", __LINE__, #x, sqlite3_errmsg(db)); return 1; } } while (0)
#define EXEC(sql) REQUIRE(sqlite3_exec(db, sql, NULL, NULL, NULL) == SQLITE_OK)
#define EXPECT(sql,value) REQUIRE(fts_expect(db, sql, value) == 0)
static int fts_checks;
static int fts_aux_calls;
static int fts_aux_destroyed;
static int fts_magic = 42;

static int fts_expect(sqlite3 *db, const char *sql, const char *expected) {
    sqlite3_stmt *stmt = NULL;
    const unsigned char *text;
    REQUIRE(sqlite3_prepare_v2(db, sql, -1, &stmt, NULL) == SQLITE_OK);
    REQUIRE(sqlite3_step(stmt) == SQLITE_ROW);
    text = sqlite3_column_text(stmt, 0);
    if (!text || strcmp((const char *)text, expected) != 0) {
        printf("FAIL FTS5 SQL %s expected [%s] actual [%s]\n", sql, expected, text ? (const char *)text : "NULL");
        sqlite3_finalize(stmt);
        return 1;
    }
    REQUIRE(sqlite3_step(stmt) == SQLITE_DONE);
    REQUIRE(sqlite3_finalize(stmt) == SQLITE_OK);
    ++fts_checks;
    return 0;
}

static void fts_aux_destroy(void *context) {
    if (context == &fts_magic) ++fts_aux_destroyed;
}
static void fts_aux(const Fts5ExtensionApi *api, Fts5Context *fts,
                    sqlite3_context *context, int argc, sqlite3_value **argv) {
    int instances = 0;
    int columns = api->xColumnCount(fts);
    (void)argv;
    if (argc != 0 || columns != 2 || api->xUserData(fts) != &fts_magic ||
        api->xInstCount(fts, &instances) != SQLITE_OK) {
        sqlite3_result_error(context, "FTS5 auxiliary API contract", -1);
        return;
    }
    ++fts_aux_calls;
    sqlite3_result_int64(context, api->xRowid(fts) * 100 + instances);
}
static int fts_token(void *context, int flags, const char *token, int length,
                     int start, int end) {
    int *count = (int *)context;
    (void)flags;
    if (length < 1 || start < 0 || end <= start || token == NULL) return SQLITE_ERROR;
    ++*count;
    return SQLITE_OK;
}

static int fts_callbacks(sqlite3 *db) {
    sqlite3_stmt *stmt = NULL;
    fts5_api *api = NULL;
    fts5_tokenizer tokenizer;
    Fts5Tokenizer *instance = NULL;
    void *tokenizer_context = NULL;
    int count = 0;
    REQUIRE(sqlite3_prepare_v2(db, "SELECT fts5(?1)", -1, &stmt, NULL) == SQLITE_OK);
    REQUIRE(sqlite3_bind_pointer(stmt, 1, &api, "fts5_api_ptr", NULL) == SQLITE_OK);
    REQUIRE(sqlite3_step(stmt) == SQLITE_ROW);
    REQUIRE(sqlite3_finalize(stmt) == SQLITE_OK);
    REQUIRE(api != NULL && api->iVersion >= 2);
    REQUIRE(api->xCreateFunction(api, "dotcc_hits", &fts_magic, fts_aux, fts_aux_destroy) == SQLITE_OK);
    EXPECT("SELECT group_concat(v) FROM (SELECT dotcc_hits(docs) AS v FROM docs WHERE docs MATCH 'quick' ORDER BY rowid)", "101,201,401");
    REQUIRE(fts_aux_calls == 3 && fts_aux_destroyed == 0);
    REQUIRE(api->xFindTokenizer(api, "unicode61", &tokenizer_context, &tokenizer) == SQLITE_OK);
    REQUIRE(tokenizer.xCreate(tokenizer_context, NULL, 0, &instance) == SQLITE_OK);
    REQUIRE(tokenizer.xTokenize(instance, &count, FTS5_TOKENIZE_QUERY, "one two three", 13, fts_token) == SQLITE_OK);
    tokenizer.xDelete(instance);
    REQUIRE(count == 3);
    REQUIRE(api->xCreateTokenizer(api, "dotcc_unicode", tokenizer_context, &tokenizer, NULL) == SQLITE_OK);
    EXEC("CREATE VIRTUAL TABLE custom_tokens USING fts5(body, tokenize='dotcc_unicode'); INSERT INTO custom_tokens VALUES('CAFÉ blue');");
    EXPECT("SELECT count(*) FROM custom_tokens WHERE custom_tokens MATCH 'cafe'", "1");
    puts("PASS FTS5 explicit auxiliary/tokenizer callbacks, pointer API and token lifetime");
    return 0;
}

int main(void) {
    sqlite3 *db = NULL;
    REQUIRE(dotcc_memory_vfs_reset() == SQLITE_OK);
    REQUIRE(sqlite3_open_v2("fts5.db", &db, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, DOTCC_MEMORY_VFS_NAME) == SQLITE_OK);
    EXPECT("SELECT sqlite_compileoption_used('ENABLE_FTS5') || ':' || sqlite_compileoption_used('ENABLE_FTS3') || ':' || sqlite_compileoption_used('ENABLE_FTS4') || ':' || sqlite_compileoption_used('OMIT_LOAD_EXTENSION')", "1:0:0:1");
    REQUIRE(sqlite3_exec(db, "CREATE VIRTUAL TABLE excluded3 USING fts3(body)", NULL, NULL, NULL) == SQLITE_ERROR);
    REQUIRE(sqlite3_exec(db, "CREATE VIRTUAL TABLE excluded4 USING fts4(body)", NULL, NULL, NULL) == SQLITE_ERROR);
    EXEC("CREATE VIRTUAL TABLE docs USING fts5(title,body,prefix='2 3'); INSERT INTO docs(rowid,title,body) VALUES(1,'alpha','quick brown fox'),(2,'beta','quick blue hare'),(3,'gamma','slow brown bear'),(4,'alpha','fox fox quick');");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH 'quick' ORDER BY rowid)", "1,2,4");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH '\"quick brown\"' ORDER BY rowid)", "1");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH 'NEAR(quick fox, 1)' ORDER BY rowid)", "1,4");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH 'title:alpha' ORDER BY rowid)", "1,4");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH 'quick NOT brown' ORDER BY rowid)", "2,4");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH '(quick AND fox) OR slow' ORDER BY rowid)", "1,3,4");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH 'bro*' ORDER BY rowid)", "1,3");
    EXPECT("SELECT group_concat(rowid) FROM (SELECT rowid FROM docs WHERE docs MATCH '^quick' ORDER BY rowid)", "1,2");
    EXPECT("SELECT rowid FROM docs WHERE docs MATCH 'fox' ORDER BY bm25(docs),rowid LIMIT 1", "4");
    EXPECT("SELECT count(*) FROM docs WHERE docs MATCH 'fox' AND rank < 0", "2");
    EXPECT("SELECT highlight(docs,1,'[',']') FROM docs WHERE docs MATCH 'brown' AND rowid=1", "quick [brown] fox");
    EXPECT("SELECT snippet(docs,1,'<','>','...',8) FROM docs WHERE docs MATCH 'brown' AND rowid=1", "quick <brown> fox");
    REQUIRE(sqlite3_exec(db, "SELECT * FROM docs WHERE docs MATCH '\"unterminated'", NULL, NULL, NULL) == SQLITE_ERROR);
    puts("PASS FTS5 phrases, NEAR, column filters, boolean/prefix/initial queries, ranking and snippets");

    EXEC("CREATE VIRTUAL TABLE unicode_docs USING fts5(body,tokenize='unicode61 remove_diacritics 2'); INSERT INTO unicode_docs VALUES('CAFÉ naïve Straße Ελληνικά');");
    EXPECT("SELECT count(*) FROM unicode_docs WHERE unicode_docs MATCH 'cafe naive'", "1");
    EXPECT("SELECT count(*) FROM unicode_docs WHERE unicode_docs MATCH 'ελληνικά'", "1");
    EXEC("CREATE VIRTUAL TABLE ascii_docs USING fts5(body,tokenize='ascii'); INSERT INTO ascii_docs VALUES('HELLO world');");
    EXPECT("SELECT count(*) FROM ascii_docs WHERE ascii_docs MATCH 'hello'", "1");
    EXEC("CREATE VIRTUAL TABLE porter_docs USING fts5(body,tokenize='porter unicode61'); INSERT INTO porter_docs VALUES('running runner runs');");
    EXPECT("SELECT count(*) FROM porter_docs WHERE porter_docs MATCH 'run'", "1");
    EXEC("CREATE VIRTUAL TABLE trigram_docs USING fts5(body,tokenize='trigram'); INSERT INTO trigram_docs VALUES('abcdefghij');");
    EXPECT("SELECT count(*) FROM trigram_docs WHERE trigram_docs MATCH 'cdef'", "1");
    EXPECT("SELECT count(*) FROM trigram_docs WHERE body LIKE '%defg%'", "1");
    EXEC("CREATE VIRTUAL TABLE vocab USING fts5vocab(docs,'row');");
    EXPECT("SELECT doc||':'||cnt FROM vocab WHERE term='fox'", "2:3");
    puts("PASS FTS5 unicode61, ascii, porter, trigram tokenizers and vocabulary");

    EXEC("CREATE TABLE source_docs(id INTEGER PRIMARY KEY,body TEXT); CREATE VIRTUAL TABLE external_docs USING fts5(body,content='source_docs',content_rowid='id'); CREATE TRIGGER source_ai AFTER INSERT ON source_docs BEGIN INSERT INTO external_docs(rowid,body) VALUES(new.id,new.body); END; CREATE TRIGGER source_ad AFTER DELETE ON source_docs BEGIN INSERT INTO external_docs(external_docs,rowid,body) VALUES('delete',old.id,old.body); END; CREATE TRIGGER source_au AFTER UPDATE ON source_docs BEGIN INSERT INTO external_docs(external_docs,rowid,body) VALUES('delete',old.id,old.body); INSERT INTO external_docs(rowid,body) VALUES(new.id,new.body); END; INSERT INTO source_docs VALUES(1,'orange apple'),(2,'pear apple');");
    EXPECT("SELECT count(*) FROM external_docs WHERE external_docs MATCH 'apple'", "2");
    EXEC("UPDATE source_docs SET body='plum' WHERE id=1; DELETE FROM source_docs WHERE id=2;");
    EXPECT("SELECT count(*) FROM external_docs WHERE external_docs MATCH 'apple'", "0");
    EXPECT("SELECT body FROM external_docs WHERE external_docs MATCH 'plum'", "plum");
    EXEC("INSERT INTO external_docs(external_docs) VALUES('rebuild'); INSERT INTO external_docs(external_docs,rank) VALUES('integrity-check',1);");
    EXEC("CREATE VIRTUAL TABLE contentless USING fts5(body,content=''); INSERT INTO contentless(rowid,body) VALUES(10,'red green');");
    EXPECT("SELECT rowid||':'||(body IS NULL) FROM contentless WHERE contentless MATCH 'green'", "10:1");
    EXEC("INSERT INTO contentless(contentless,rowid,body) VALUES('delete',10,'red green');");
    EXPECT("SELECT count(*) FROM contentless WHERE contentless MATCH 'green'", "0");
    EXEC("CREATE VIRTUAL TABLE contentless_delete USING fts5(body,content='',contentless_delete=1); INSERT INTO contentless_delete(rowid,body) VALUES(20,'violet'); DELETE FROM contentless_delete WHERE rowid=20;");
    EXPECT("SELECT count(*) FROM contentless_delete WHERE contentless_delete MATCH 'violet'", "0");
    puts("PASS FTS5 external-content triggers/rebuild/integrity and contentless deletion");

    EXEC("BEGIN; INSERT INTO docs VALUES('delta','rollbackword'); SAVEPOINT f; DELETE FROM docs WHERE rowid=1; ROLLBACK TO f; RELEASE f; ROLLBACK;");
    EXPECT("SELECT count(*) FROM docs WHERE docs MATCH 'rollbackword'", "0");
    EXPECT("SELECT count(*) FROM docs WHERE docs MATCH 'quick'", "3");
    EXEC("BEGIN; INSERT INTO docs(rowid,title,body) VALUES(5,'delta','committedword'); COMMIT; INSERT INTO docs(docs) VALUES('optimize'); INSERT INTO docs(docs) VALUES('integrity-check');");
    EXPECT("SELECT rowid FROM docs WHERE docs MATCH 'committedword'", "5");
    REQUIRE(fts_callbacks(db) == 0);
    EXPECT("SELECT json_extract(jsonb_object('hits',(SELECT count(*) FROM docs WHERE docs MATCH 'quick')),'$.hits')", "3");
    EXPECT("PRAGMA integrity_check", "ok");
    REQUIRE(sqlite3_close(db) == SQLITE_OK);
    db = NULL;
    REQUIRE(fts_aux_destroyed == 1);
    REQUIRE(sqlite3_open_v2("fts5.db", &db, SQLITE_OPEN_READWRITE, DOTCC_MEMORY_VFS_NAME) == SQLITE_OK);
    EXPECT("SELECT rowid FROM docs WHERE docs MATCH 'committedword'", "5");
    REQUIRE(sqlite3_close(db) == SQLITE_OK);
    db = NULL;
    REQUIRE(dotcc_memory_vfs_handle_count() == 0);
    REQUIRE(dotcc_memory_vfs_reset() == SQLITE_OK);
    REQUIRE(sqlite3_shutdown() == SQLITE_OK);
    printf("PASS FTS5 transactions, savepoints, optimize, JSONB join, reopen and cleanup (%d assertions)\n", fts_checks);
    return 0;
}
