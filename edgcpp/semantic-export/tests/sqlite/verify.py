#!/usr/bin/env python3
"""Compile and execute semantic-export SQLite against its native C snapshot."""
import argparse, hashlib, json, pathlib, subprocess, time
p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--package',type=pathlib.Path,required=True)
p.add_argument('--generated',type=pathlib.Path,required=True)
p.add_argument('--output',type=pathlib.Path,required=True)
p.add_argument('--nativeaot',action='store_true')
a=p.parse_args();a.package=a.package.resolve();a.generated=a.generated.resolve();a.output=a.output.resolve();a.output.mkdir(parents=True,exist_ok=False)
checks=[]
def run(name,command,timeout=900):
 command=list(map(str,command));start=time.monotonic()
 (a.output/(name+'.command.json')).write_text(json.dumps(command,indent=2)+'\n')
 with (a.output/(name+'.stdout')).open('w') as out,(a.output/(name+'.stderr')).open('w') as err:
  r=subprocess.run(command,stdout=out,stderr=err,timeout=timeout)
 checks.append({'name':name,'exit':r.returncode,'seconds':round(time.monotonic()-start,3)})
 if r.returncode:raise RuntimeError(f'{name} failed: see {a.output}/{name}.stdout/.stderr')
 return (a.output/(name+'.stdout')).read_text()
manifest=json.loads((a.package/'package.json').read_text())
native=a.output/'native';native.mkdir()
for source in manifest['sources']:
 if source.get('available') and pathlib.Path(source['originalPath']).name in ['sqlite3.c','sqlite3.h','memory_vfs.c','memory_vfs.h']:
  data=(a.package/source['path']).read_bytes();assert hashlib.sha256(data).hexdigest()==source['sha256']
  (native/pathlib.Path(source['originalPath']).name).write_bytes(data)
commands=[
('exec','CREATE TABLE items(id INTEGER PRIMARY KEY, value TEXT, amount REAL); INSERT INTO items VALUES(1,\'alpha\',1.25),(2,\'βeta\',-2.5),(3,NULL,0);'),
('query','SELECT id, quote(value), printf(\'%!.17g\',amount) FROM items ORDER BY id;'),
('query','WITH RECURSIVE n(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM n WHERE x<100) SELECT sum(x),avg(x),count(*) FROM n;'),
('exec','BEGIN; UPDATE items SET value=\'changed\' WHERE id=1; ROLLBACK;'),
('query','SELECT group_concat(value,\'|\'),total(amount),min(amount),max(amount) FROM items;'),
('exec','CREATE INDEX items_value ON items(value); CREATE TABLE audit(v); CREATE TRIGGER log_insert AFTER INSERT ON items BEGIN INSERT INTO audit VALUES(new.id); END; INSERT INTO items VALUES(4,\'four\',16.0);'),
('query','SELECT v FROM audit;'),
('query','SELECT id,row_number() OVER(ORDER BY amount),sum(amount) OVER(ORDER BY id) FROM items ORDER BY id;'),
('query','SELECT json_extract(jsonb(\'{"a":[1,2,3],"s":"hello"}\'),\'$.a[2]\'),json_valid(\'{"x":1}\'),json_array_length(\'[1,null,3]\');'),
('exec','CREATE VIRTUAL TABLE docs USING fts5(title,body); INSERT INTO docs VALUES(\'one\',\'semantic export compiler\'),(\'two\',\'database engine\'),(\'three\',\'compiler database\');'),
('query','SELECT title FROM docs WHERE docs MATCH \'compiler\' ORDER BY title;'),
('query','SELECT printf(\'%.6f\',sin(0.5)),pow(2,10),sqrt(81),ceil(-1.5),floor(-1.5),mod(17,5);'),
('query','SELECT percentile(amount,50),median(amount) FROM items;'),
('query','SELECT hex(zeroblob(8)),length(cast(\'βeta\' AS blob)),unicode(\'β\');'),
('query','SELECT date(\'2024-02-29\',\'+1 day\'),strftime(\'%s\',\'1970-01-01\'),unixepoch(\'2025-01-01\');'),
('query','SELECT sqlite_version(),sqlite_compileoption_used(\'ENABLE_FTS5\'),sqlite_compileoption_used(\'THREADSAFE=0\');'),
('query','PRAGMA integrity_check;'),
('exec',"CREATE TABLE bulk(k INTEGER PRIMARY KEY,t TEXT,v INTEGER); WITH RECURSIVE n(x) AS (VALUES(1) UNION ALL SELECT x+1 FROM n WHERE x<2000) INSERT INTO bulk SELECT x,printf('value-%06d',x),x%17 FROM n; CREATE INDEX bulk_v ON bulk(v);"),
('query',"SELECT v,count(*),sum(k) FROM bulk GROUP BY v ORDER BY v;"),
('exec',"BEGIN; DELETE FROM bulk WHERE k%3=0; UPDATE bulk SET t=upper(t) WHERE k%7=0; COMMIT; VACUUM;"),
('query',"SELECT count(*),sum(length(t)),sum(k) FROM bulk;"),
('query',"PRAGMA integrity_check;"),
('exec',"INSERT INTO docs VALUES('unicode','naïve café Ελληνικά βήτα');"),
('query',"SELECT title FROM docs WHERE docs MATCH 'cafe OR βήτα' ORDER BY title;"),
('exec','CREATE TABLE bindings(i INTEGER,r REAL,t TEXT,b BLOB);'),
]
c=r'''#include "sqlite3.c"
#include "memory_vfs.c"
#include <stdio.h>
static void execsql(sqlite3 *db,const char *sql) { int rc=sqlite3_exec(db,sql,0,0,0); if(rc){fprintf(stderr,"exec %d %s\n",rc,sqlite3_errmsg(db));exit(1);} }
static void query(sqlite3 *db,const char *sql,int id) { sqlite3_stmt *s=0;int rc=sqlite3_prepare_v2(db,sql,-1,&s,0);if(rc){fprintf(stderr,"prepare %d %s\n",rc,sqlite3_errmsg(db));exit(1);}while((rc=sqlite3_step(s))==SQLITE_ROW){printf("%d",id);for(int i=0;i<sqlite3_column_count(s);++i){const unsigned char *v=sqlite3_column_text(s,i);printf("\t%s",v?(const char*)v:"<NULL>");}puts("");}if(rc!=SQLITE_DONE)exit(2);if(sqlite3_finalize(s))exit(3);}
int main(void){sqlite3 *db=0; if(sqlite3_open(":memory:",&db))return 4;
'''
cs=r'''using System;
using System.Text;
using System.Runtime.InteropServices;
using static Edg.Semantic.Generated.SqliteCore;
unsafe class Program {
static string Text(sbyte* p)=>p==null?"<NULL>":Marshal.PtrToStringUTF8((nint)p)!;
static void Exec(sqlite3* db,string sql){fixed(byte* p=Encoding.UTF8.GetBytes(sql+"\0")){int rc=sqlite3_exec(db,(sbyte*)p,null,null,null);if(rc!=0)throw new Exception("exec "+rc+" "+Text(sqlite3_errmsg(db)));}}
static void Query(sqlite3* db,string sql,int id){sqlite3_stmt* s=null;fixed(byte* p=Encoding.UTF8.GetBytes(sql+"\0")){int rc=sqlite3_prepare_v2(db,(sbyte*)p,-1,&s,null);if(rc!=0)throw new Exception("prepare "+rc+" "+Text(sqlite3_errmsg(db)));}int step;while((step=sqlite3_step(s))==100){Console.Write(id);for(int i=0;i<sqlite3_column_count(s);++i)Console.Write("\t"+Text((sbyte*)sqlite3_column_text(s,i)));Console.WriteLine();}if(step!=101)throw new Exception("step "+step);if(sqlite3_finalize(s)!=0)throw new Exception("finalize");}
static int Main(){sqlite3* db=null;fixed(byte* p=Encoding.UTF8.GetBytes(":memory:\0")){int rc=sqlite3_open((sbyte*)p,&db);if(rc!=0)throw new Exception("open "+rc+" "+Text(sqlite3_errmsg(db)));}
'''
for i,(mode,sql) in enumerate(commands):
 literal=json.dumps(sql,ensure_ascii=False)
 if mode=='exec': c+=f'execsql(db,{literal});\n';cs+=f'Exec(db,{literal});\n'
 else:c+=f'query(db,{literal},{i});\n';cs+=f'Query(db,{literal},{i});\n'
c+=r'''
sqlite3_stmt *stmt=0;
if(sqlite3_prepare_v2(db,"INSERT INTO bindings VALUES(?,?,?,?)",-1,&stmt,0))return 5;
unsigned char blob[]={0,1,127,128,255};
sqlite3_bind_int64(stmt,1,-9223372036854775807LL);sqlite3_bind_double(stmt,2,1.23456789012345);
sqlite3_bind_text(stmt,3,"varargs UTF-8 β",-1,SQLITE_TRANSIENT);sqlite3_bind_blob(stmt,4,blob,5,SQLITE_TRANSIENT);
if(sqlite3_step(stmt)!=SQLITE_DONE)return 6;sqlite3_finalize(stmt);
query(db,"SELECT i,printf('%!.17g',r),t,hex(b) FROM bindings",100);
char *formatted=sqlite3_mprintf("%lld:%Q:%!.3f",-9223372036854775807LL,"a'b",1.25);printf("format\t%s\n",formatted);sqlite3_free(formatted);
if(sqlite3_close(db))return 7;
if(sqlite3_open("semantic.db",&db))return 8;execsql(db,"CREATE TABLE durable(v);INSERT INTO durable VALUES('retained');");sqlite3_close(db);
if(sqlite3_open("semantic.db",&db))return 9;query(db,"SELECT v FROM durable",101);sqlite3_close(db);
if(sqlite3_shutdown())return 10;puts("complete");return 0;}
'''
cs+=r'''
sqlite3_stmt* stmt=null;
fixed(byte* p=Encoding.UTF8.GetBytes("INSERT INTO bindings VALUES(?,?,?,?)\0")){if(sqlite3_prepare_v2(db,(sbyte*)p,-1,&stmt,null)!=0)throw new Exception("bind prepare");}
byte* blob=stackalloc byte[]{0,1,127,128,255};
sqlite3_bind_int64(stmt,1,-9223372036854775807L);sqlite3_bind_double(stmt,2,1.23456789012345);
fixed(byte* p=Encoding.UTF8.GetBytes("varargs UTF-8 β\0"))sqlite3_bind_text(stmt,3,(sbyte*)p,-1,(delegate* managed<void*,void>)(nint)(-1));
sqlite3_bind_blob(stmt,4,blob,5,(delegate* managed<void*,void>)(nint)(-1));
if(sqlite3_step(stmt)!=101)throw new Exception("insert step");sqlite3_finalize(stmt);
Query(db,"SELECT i,printf('%!.17g',r),t,hex(b) FROM bindings",100);
fixed(byte* format=Encoding.UTF8.GetBytes("%lld:%Q:%!.3f\0"))fixed(byte* text=Encoding.UTF8.GetBytes("a'b\0")){
sbyte* formatted=sqlite3_mprintf((sbyte*)format,[__CArg.From(-9223372036854775807L),__CArg.Pointer(text),__CArg.From(1.25)]);Console.WriteLine("format\t"+Text(formatted));sqlite3_free(formatted);}
if(sqlite3_close(db)!=0)throw new Exception("close");
fixed(byte* p=Encoding.UTF8.GetBytes("semantic.db\0")){if(sqlite3_open((sbyte*)p,&db)!=0)throw new Exception("named open");}Exec(db,"CREATE TABLE durable(v);INSERT INTO durable VALUES('retained');");sqlite3_close(db);
fixed(byte* p=Encoding.UTF8.GetBytes("semantic.db\0")){if(sqlite3_open((sbyte*)p,&db)!=0)throw new Exception("named reopen");}Query(db,"SELECT v FROM durable",101);sqlite3_close(db);
if(sqlite3_shutdown()!=0)throw new Exception("shutdown");Console.WriteLine("complete");return 0;}}
'''
(native/'oracle.c').write_text(c)
consumer=a.output/'consumer';consumer.mkdir();(consumer/'Program.cs').write_text(cs)
(consumer/'Consumer.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>enable</Nullable><IsAotCompatible>true</IsAotCompatible></PropertyGroup><ItemGroup><ProjectReference Include="{a.generated/'Generated.csproj'}" /></ItemGroup></Project>''')
defines=[value[2:] for value in manifest['invocation']['arguments'] if value.startswith('-DSQLITE_')]
if not defines:raise ValueError('Package does not record its SQLite feature definitions')
run('native-build',['gcc','-std=c17','-O2',*['-D'+d for d in defines],native/'oracle.c','-lm','-o',native/'oracle'])
expected=run('native-run',[native/'oracle'])
run('jit-build',['dotnet','build',consumer/'Consumer.csproj','-c','Release','-o',a.output/'jit'])
actual=run('jit-run',['dotnet',a.output/'jit/Consumer.dll'])
if actual!=expected:raise AssertionError('Native/JIT output differs; see retained stdout')
if a.nativeaot:
 run('aot-build',['dotnet','publish',consumer/'Consumer.csproj','-c','Release','-r','linux-x64','-p:PublishAot=true','-o',a.output/'aot'],timeout=1800)
 actual=run('aot-run',[a.output/'aot/Consumer'])
 if actual!=expected:raise AssertionError('Native/NativeAOT output differs; see retained stdout')
(a.output/'results.json').write_text(json.dumps({'checks':checks,'passed':len(checks),'observations':len(expected.splitlines()),'nativeAot':a.nativeaot},indent=2)+'\n')
print('PASS SQLite semantic export:',len(checks),'checks,',len(expected.splitlines()),'observations')
