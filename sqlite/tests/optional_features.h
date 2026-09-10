/* Included by the native/translated API corpus; CHECK, OK and expect_text are
 * supplied by api_native.c. Assertions use tolerances, not libc float formatting. */
static int optional_features_contract(sqlite3 *db) {
    sqlite3_stmt *stmt = NULL;
    const char *invalid[] = {
        "SELECT percentile(1,-1)", "SELECT percentile(1,101)",
        "SELECT percentile(1,NULL)", "SELECT percentile_cont(1,1.1)",
        "SELECT percentile_disc(1,-0.1)", "SELECT median('invalid')",
        "SELECT median(1e999)",
        "WITH t(x,p) AS (VALUES(1,25),(2,75)) SELECT percentile(x,p) FROM t"
    };
    unsigned short main16[] = {'m','a','i','n',0};
    unsigned short table16[] = {'m','e','t','a','d','a','t','a','_','b','a','s','e',0};
    unsigned short origin16[] = {'n','a',0xef,'v','e',0};
    int i;
    CHECK(expect_text(db, "SELECT sqlite_compileoption_used('ENABLE_MATH_FUNCTIONS') AND sqlite_compileoption_used('ENABLE_PERCENTILE') AND sqlite_compileoption_used('ENABLE_COLUMN_METADATA')", "1") == 0);
    CHECK(expect_text(db, "SELECT sqrt(81)=9 AND pow(2,10)=1024 AND power(2,3)=8 AND mod(7,3)=1 AND ceil(-1.5)=-1 AND ceiling(1.5)=2 AND floor(-1.5)=-2 AND trunc(-1.5)=-1", "1") == 0);
    CHECK(expect_text(db, "SELECT abs(log(100)-2)<1e-12 AND abs(log10(1000)-3)<1e-12 AND abs(log2(32)-5)<1e-12 AND abs(log(2,8)-3)<1e-12 AND abs(ln(exp(2))-2)<1e-12", "1") == 0);
    CHECK(expect_text(db, "SELECT abs(sin(pi()/2)-1)<1e-12 AND abs(cos(pi())+1)<1e-12 AND abs(tan(pi()/4)-1)<1e-12 AND abs(asin(1)-pi()/2)<1e-12 AND abs(acos(-1)-pi())<1e-12 AND abs(atan(1)-pi()/4)<1e-12 AND abs(atan2(1,1)-pi()/4)<1e-12", "1") == 0);
    CHECK(expect_text(db, "SELECT abs(asinh(sinh(1))-1)<1e-12 AND abs(acosh(cosh(1))-1)<1e-12 AND abs(atanh(tanh(0.5))-0.5)<1e-12 AND abs(degrees(pi())-180)<1e-12 AND abs(radians(180)-pi())<1e-12", "1") == 0);
    CHECK(expect_text(db, "SELECT sqrt(-1) IS NULL AND sqrt(NULL) IS NULL AND sqrt('invalid') IS NULL AND log(0) IS NULL AND log(1,10) IS NULL AND acos(2) IS NULL AND mod(1,0) IS NULL", "1") == 0);
    puts("PASS API math functions arithmetic logs trigonometry hyperbolic and domain NULLs");

    CHECK(expect_text(db, "WITH t(x) AS (VALUES(9),(1),(NULL),(3),(3)) SELECT median(x)=3 AND percentile(x,25)=2.5 AND percentile_cont(x,0.75)=4.5 AND percentile_disc(x,0.75)=3 AND percentile(x,0)=1 AND percentile(x,100)=9 FROM t", "1") == 0);
    CHECK(expect_text(db, "SELECT median(x) IS NULL AND percentile(x,50) IS NULL FROM (SELECT 1 AS x WHERE 0)", "1") == 0);
    CHECK(expect_text(db, "SELECT median(NULL) IS NULL AND percentile(NULL,50) IS NULL AND percentile_cont(NULL,0.5) IS NULL AND percentile_disc(NULL,0.5) IS NULL", "1") == 0);
    CHECK(expect_text(db, "SELECT median(7)=7 AND percentile(7,25)=7 AND percentile_cont(7,0.5)=7 AND percentile_disc(7,1)=7", "1") == 0);
    CHECK(expect_text(db, "WITH t(i,x) AS (VALUES(1,1),(2,8),(3,3),(4,6),(5,9)) SELECT group_concat(m,',') FROM (SELECT median(x) OVER (ORDER BY i ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING) AS m FROM t ORDER BY i)", "4.5,3.0,6.0,6.0,7.5") == 0);
    CHECK(expect_text(db, "WITH t(g,x) AS (VALUES('a',1),('a',3),('b',2),('b',8)) SELECT group_concat(m,',') FROM (SELECT median(x) AS m FROM t GROUP BY g ORDER BY g)", "2.0,5.0") == 0);
    CHECK(expect_text(db, "WITH RECURSIVE t(x) AS (VALUES(2000) UNION ALL SELECT x-1 FROM t WHERE x>1) SELECT median(x)=1000.5 AND percentile_cont(x,0.5)=1000.5 FROM t", "1") == 0);
    for (i = 0; i < (int)(sizeof(invalid)/sizeof(invalid[0])); ++i)
        CHECK(sqlite3_exec(db, invalid[i], NULL, NULL, NULL) == SQLITE_ERROR);
    CHECK(expect_text(db, "SELECT median(42)", "42.0") == 0);
    puts("PASS API percentile aggregates windows groups sorting NULLs and invalid-input recovery");

    OK(sqlite3_exec(db, "CREATE TABLE metadata_base(id INTEGER PRIMARY KEY,\"naïve\" TEXT); INSERT INTO metadata_base VALUES(1,'one'); CREATE VIEW metadata_view AS SELECT id,\"naïve\" AS label FROM metadata_base; ATTACH ':memory:' AS extra; CREATE TABLE extra.metadata_other(value TEXT); INSERT INTO extra.metadata_other VALUES('two');", NULL, NULL, NULL));
    OK(sqlite3_prepare_v2(db, "SELECT v.label AS renamed,o.value,v.id+1 AS expression FROM metadata_view AS v CROSS JOIN extra.metadata_other AS o", -1, &stmt, NULL));
    CHECK(strcmp(sqlite3_column_name(stmt,0), "renamed") == 0);
    CHECK(strcmp(sqlite3_column_database_name(stmt,0), "main") == 0);
    CHECK(strcmp(sqlite3_column_table_name(stmt,0), "metadata_base") == 0);
    CHECK(strcmp(sqlite3_column_origin_name(stmt,0), "naïve") == 0);
    CHECK(strcmp(sqlite3_column_decltype(stmt,0), "TEXT") == 0);
    CHECK(memcmp(sqlite3_column_database_name16(stmt,0), main16, sizeof(main16)) == 0);
    CHECK(memcmp(sqlite3_column_table_name16(stmt,0), table16, sizeof(table16)) == 0);
    CHECK(memcmp(sqlite3_column_origin_name16(stmt,0), origin16, sizeof(origin16)) == 0);
    CHECK(strcmp(sqlite3_column_database_name(stmt,1), "extra") == 0);
    CHECK(strcmp(sqlite3_column_table_name(stmt,1), "metadata_other") == 0);
    CHECK(strcmp(sqlite3_column_origin_name(stmt,1), "value") == 0);
    CHECK(sqlite3_column_database_name(stmt,2) == NULL);
    CHECK(sqlite3_column_table_name(stmt,2) == NULL);
    CHECK(sqlite3_column_origin_name(stmt,2) == NULL);
    CHECK(sqlite3_column_database_name16(stmt,2) == NULL);
    CHECK(sqlite3_column_table_name16(stmt,2) == NULL);
    CHECK(sqlite3_column_origin_name16(stmt,2) == NULL);
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    OK(sqlite3_reset(stmt));
    CHECK(strcmp(sqlite3_column_origin_name(stmt,0), "naïve") == 0);
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    CHECK(sqlite3_step(stmt) == SQLITE_DONE);
    OK(sqlite3_finalize(stmt));
    OK(sqlite3_exec(db, "DETACH extra; DROP VIEW metadata_view; DROP TABLE metadata_base;", NULL, NULL, NULL));
    puts("PASS API column metadata UTF8 UTF16 aliases views joins attached databases expressions and reset");
    return 0;
}
