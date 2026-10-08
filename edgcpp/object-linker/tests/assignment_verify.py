"""C++17 RHS-first sequencing in source assignments and surrounding control flow."""
import sqlite3


def verify(inputs, out, run, check, compile_source, link, execute):
    for name in ['cpp_assignment_order', 'project_cpp_assignment_lifting']:
        source = inputs / (name + '.cpp')
        obj = compile_source(name, source)
        for level in [0, 2]:
            native = out / (name + '-native-' + str(level))
            run(name + '-native-build-' + str(level), ['c++', '-std=c++17', '-O' + str(level), source, '-o', native])
            run(name + '-native-run-' + str(level), [native])
        with sqlite3.connect(obj) as db:
            check(name + '-owned-rhs-first-order', db.execute(
                "SELECT count(*) FROM nodes WHERE kind IN ('assign','compound-assign') AND evaluation_order='right-to-left'").fetchone()[0] > 0)
        execute(name, link('link-' + name, [obj]))
