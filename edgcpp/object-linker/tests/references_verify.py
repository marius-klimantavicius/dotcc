"""Original C++ reference/receiver identity and linked execution contracts."""
import sqlite3


def verify(inputs, out, tool, run, check, compile_source, link, execute, mutate):
    source = inputs / 'project_cpp_references_members.cpp'
    obj = compile_source('cpp-references-members', source, extra=['--exceptions', '--rtti'])
    for optimization in ['0', '2']:
        native = out / ('cpp-references-native-' + optimization)
        run('cpp-references-native-build-' + optimization,
            ['c++', '-std=c++17', '-O' + optimization, source, '-o', native])
        run('cpp-references-native-' + optimization, [native])
    directory = link('link-cpp-references-members', [obj])
    execute('cpp-references-members', directory)
    with sqlite3.connect(obj) as db:
        check('source-reference-kinds', {r[0] for r in db.execute(
            "SELECT DISTINCT kind FROM source_types WHERE kind IN ('lvalue-reference','rvalue-reference')")}
            == {'lvalue-reference', 'rvalue-reference'})
        check('original-member-initializer', db.execute(
            "SELECT has_initializer FROM source_record_fields WHERE source_name='value'").fetchone()[0] == 1)
        check('owned-this-parameters', db.execute(
            "SELECT count(*) FROM parameters WHERE parameter_role='this' AND ordinal=0 AND source_ordinal IS NULL").fetchone()[0] >= 4)
        check('receiver-reference-qualifiers', {r[0] for r in db.execute(
            "SELECT DISTINCT object_ref_qualifier FROM source_routine_types WHERE object_type_id IS NOT NULL")}
            >= {'lvalue', 'rvalue'})
        check('receiver-cv-qualifiers', db.execute(
            "SELECT max(object_is_const),max(object_is_volatile) FROM source_routine_types").fetchone() == (1, 1))
        check('this-not-fabricated-source-parameter', db.execute("""
            SELECT count(*) FROM bindings b JOIN parameters p
            ON p.entity_id=b.function_id AND p.ordinal=b.ordinal
            JOIN source_declarations d ON d.binding_id=b.id
            WHERE b.kind='parameter' AND p.parameter_role='this'
            """).fetchone()[0] == 0)

    bad = mutate('this-missing-synthetic-proof', obj, [("""
        UPDATE bindings SET is_synthetic=0 WHERE id=(SELECT b.id FROM bindings b
        JOIN parameters p ON p.entity_id=b.function_id AND p.ordinal=b.ordinal
        WHERE b.kind='parameter' AND p.parameter_role='this' LIMIT 1)
        """, ())])
    run('reject-this-missing-synthetic-proof', ['dotnet', tool, 'verify', bad], None)

    # These have the same physical pointer ABI. Linking must still distinguish
    # a source reference from a source pointer, including nested callbacks.
    incompatible = [
        ('direct', 'extern "C" int reference_api(int &v){return v;}',
         'extern "C" int reference_api(int*);int main(){int x=2;return reference_api(&x)-2;}'),
        ('callback', 'extern "C" int reference_api(int (*f)(int&),int *x){return f(*x);}',
         'extern "C" int reference_api(int(*)(int*),int*);int f(int*x){return *x;}int main(){int x=2;return reference_api(f,&x)-2;}'),
    ]
    for name, provider_code, consumer_code in incompatible:
        objects = []
        for role, code in [('provider', provider_code), ('consumer', consumer_code)]:
            name_role = 'reference-mismatch-' + name + '-' + role
            path = inputs / (name_role + '.cpp')
            path.write_text(code + '\n')
            objects.append(compile_source(name_role, path))
        link('reject-reference-source-signature-' + name, objects,
             expected=None, contains='Original semantic signature mismatch')

    provider = inputs / 'reference-compatible-provider.cpp'
    provider.write_text('''typedef int &IntRef;
extern "C" int compatible_ptr(int *const value){return *value;}
extern "C" int compatible_function(int fn(int),int value){return fn(value);}
extern "C" int compatible_array(int (&value)[3]){return value[0]+value[2];}
extern "C" int compatible_alias(IntRef &&value){return ++value;}
''')
    consumer = inputs / 'reference-compatible-consumer.cpp'
    consumer.write_text('''extern "C" int compatible_ptr(int*);
extern "C" int compatible_function(int(*fn)(int),int value);
extern "C" int compatible_array(int (&value)[3]);
extern "C" int compatible_alias(int &value);
int twice(int x){return x*2;}
int main(){int x=7,a[3]={2,3,5};return compatible_ptr(&x)==7 &&
compatible_function(twice,4)==8 && compatible_array(a)==7 &&
compatible_alias(x)==8 && x==8 ? 0:1;}
''')
    pair = [compile_source('reference-compatible-provider', provider),
            compile_source('reference-compatible-consumer', consumer)]
    run('reference-compatible-native-build', ['c++', '-std=c++17', '-O2', provider, consumer,
                                             '-o', out / 'reference-compatible-native'])
    run('reference-compatible-native', [out / 'reference-compatible-native'])
    execute('reference-compatible', link('link-reference-compatible', pair))


    # Observation provenance differs across source languages; compatible direct
    # callable semantics must still link, including nested callback signatures.
    c_source = inputs / 'source-abi-c-provider.c'
    c_source.write_text('int mixed_apply(int (*fn)(int), int value) { return fn(value) + 1; }\n')
    cpp_source = inputs / 'source-abi-cpp-consumer.cpp'
    cpp_source.write_text('extern "C" int mixed_apply(int (*)(int), int);\n'
                          'int twice_mixed(int x) { return x * 2; }\n'
                          'int main() { return mixed_apply(twice_mixed, 8) == 17 ? 0 : 1; }\n')
    c_object = compile_source('source-abi-c-provider', c_source, std='c11')
    cpp_object = compile_source('source-abi-cpp-consumer', cpp_source)
    for role, object_path, language, proof in [
            ('c', c_object, 'c', 'source-c-signature'),
            ('cpp', cpp_object, 'c++', 'observed-routine-lowering')]:
        with sqlite3.connect(object_path) as db:
            check('source-abi-' + role + '-language-proof',
                  db.execute('SELECT source_language FROM object_info').fetchone() == (language,)
                  and {row[0] for row in db.execute('SELECT DISTINCT capture_proof FROM source_routine_abis')} == {proof})
    run('source-abi-mixed-native-c', ['cc', '-std=c11', '-O2', '-c', c_source,
                                     '-o', out / 'source-abi-c-provider.o'])
    run('source-abi-mixed-native-link', ['c++', '-std=c++17', '-O2', cpp_source,
          out / 'source-abi-c-provider.o', '-o', out / 'source-abi-mixed-native'])
    run('source-abi-mixed-native', [out / 'source-abi-mixed-native'])
    execute('source-abi-mixed', link('link-source-abi-mixed', [c_object, cpp_object]))
