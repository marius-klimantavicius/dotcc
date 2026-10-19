/* Same-pin native embedding oracle. This file is never a product backend. */
#include <Python.h>
#include <stdio.h>
#include <string.h>

static int callbacks;
static PyObject *host_double(PyObject *self, PyObject *value) {
    long x = PyLong_AsLong(value);
    if (x == -1 && PyErr_Occurred()) return NULL;
    callbacks++;
    return PyLong_FromLong(x * 2);
}
static PyMethodDef methods[] = {{"double", host_double, METH_O, "Double an integer."}, {NULL}};
static struct PyModuleDef module = {PyModuleDef_HEAD_INIT, "host", NULL, -1, methods};
static PyObject *PyInit_host(void) { return PyModule_Create(&module); }

int main(int argc, char **argv) {
    if (argc < 3) return 24;
    if (PyImport_AppendInittab("host", PyInit_host) < 0) return 24;
    const int deliberate_assertion = argc > 3 && !strcmp(argv[3], "assertion");
    const int deliberate_harness = argc > 3 && !strcmp(argv[3], "harness");
    for (int cycle = 0; cycle < 2; cycle++) {
        PyConfig config;
        PyConfig_InitIsolatedConfig(&config);
        config.install_signal_handlers = 0;
        config.site_import = 0;
        config.write_bytecode = 0;
        config.module_search_paths_set = 1;
        wchar_t *lib = Py_DecodeLocale(argv[1], NULL);
        if (!lib) return 24;
        PyStatus status = PyWideStringList_Append(&config.module_search_paths, lib);
        PyMem_RawFree(lib);
        if (!PyStatus_Exception(status)) status = Py_InitializeFromConfig(&config);
        PyConfig_Clear(&config);
        if (PyStatus_Exception(status)) { fprintf(stderr, "initialization: %s\n", status.err_msg); return 24; }
        callbacks = 0;
        PyObject *main = PyImport_AddModule("__main__");
        PyObject *globals = main ? PyModule_GetDict(main) : NULL;
        PyObject *root = PyUnicode_DecodeFSDefault(argv[2]);
        if (!globals || !root || PyDict_SetItemString(globals, "work", root) < 0) return 24;
        Py_DECREF(root);
        const char *program =
            "import os,sys,json,host,threading,contextvars,gc,weakref,math,struct,random,re,collections,functools,itertools,time,io,traceback\n"
            "os.makedirs(work,exist_ok=True)\n"
            "os.makedirs(work+'/app',exist_ok=True)\n"
            "open(work+'/app/__init__.py','w').write('def transform(values,callback):\\n return [callback(x) for x in values]\\n')\n"
            "sys.path.insert(0,work)\n"
            "import app\n"
            "open(work+'/input.json','w',encoding='utf-8').write('{\"items\":[2,3,5]}')\n"
            "values=app.transform(json.load(open(work+'/input.json',encoding='utf-8'))['items'],host.double)\n"
            "payload=json.dumps({'sum':sum(values),'values':values},sort_keys=True,separators=(',',':'))\n"
            "open(work+'/output.json','w',encoding='utf-8').write(payload)\n"
            "assert open(work+'/output.json','rb').read()==b'{\"sum\":20,\"values\":[4,6,10]}'\n"
            "assert app.transform([7],lambda x:x+1)==[8]\n"
            "stream=io.StringIO()\n"
            "print('captured',file=stream); assert stream.getvalue()=='captured\\n'\n"
            "try:\n raise ValueError('oracle-error')\n"
            "except ValueError as error:\n assert 'ValueError: oracle-error' in ''.join(traceback.format_exception(error))\n"
            "local=threading.local(); barrier=threading.Barrier(3); results=[]\n"
            "def worker(value):\n local.value=value\n barrier.wait(timeout=5)\n results.append((local.value,threading.get_ident()))\n"
            "threads=[threading.Thread(target=worker,args=(x,)) for x in (11,17)]\n"
            "for thread in threads: thread.start()\n"
            "barrier.wait(timeout=5)\n"
            "for thread in threads: thread.join(timeout=5); assert not thread.is_alive()\n"
            "assert sorted(x for x,_ in results)==[11,17] and len({t for _,t in results})==2\n"
            "class Node: pass\n"
            "node=Node(); node.link=node; ref=weakref.ref(node); del node; gc.collect(); assert ref() is None\n"
            "print('native-cycle-ok',payload)\n";
        PyObject *result = PyRun_StringFlags(program, Py_file_input, globals, globals, NULL);
        if (!result) { PyErr_Print(); Py_FinalizeEx(); return 23; }
        Py_DECREF(result);
        if (callbacks != 3) { fprintf(stderr, "callback count: %d\n", callbacks); Py_FinalizeEx(); return 23; }
        if (deliberate_assertion) {
            result = PyRun_StringFlags("assert False, 'deliberate native control'", Py_file_input, globals, globals, NULL);
            if (result) { Py_DECREF(result); return 25; }
            PyErr_Print(); Py_FinalizeEx(); return 23;
        }
        if (deliberate_harness) { fprintf(stderr,"deliberate harness error\n"); Py_FinalizeEx(); return 24; }
        if (Py_FinalizeEx() < 0) return 24;
    }
    puts("native-workflow-passed cycles=2 callbacks=6 real-workers=4");
    return 0;
}
