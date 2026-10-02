//type: fp
//options: 
# 0 "./plugin/gil-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./plugin/gil-1.c"




# 1 "./plugin/gil.h" 1

typedef struct PyThreadState PyThreadState;
extern PyThreadState * PyEval_SaveThread(void);
extern void PyEval_RestoreThread(PyThreadState *);
# 16 "./plugin/gil.h"
typedef struct _object {
    int ob_refcnt;
} PyObject;



extern void _Py_Dealloc(PyObject *);
# 6 "./plugin/gil-1.c" 2

void test_1 (void)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();
  PyEval_RestoreThread(_save); }
}

void test_2 (PyObject *obj)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();

  do { (((PyObject*)(obj)))->ob_refcnt++; } while (0);;
  do { if (--(((PyObject*)(obj)))->ob_refcnt == 0) { _Py_Dealloc(((PyObject*)(obj))); } } while (0);

  PyEval_RestoreThread(_save); }
}

void test_3 (PyObject *obj)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();

  { PyThreadState *_save; _save = PyEval_SaveThread();
  PyEval_RestoreThread(_save); }

  PyEval_RestoreThread(_save); }
}

void test_4 (PyObject *obj)
{

  { PyThreadState *_save; _save = PyEval_SaveThread();
  PyEval_RestoreThread(_save); }

  { PyThreadState *_save; _save = PyEval_SaveThread();
  PyEval_RestoreThread(_save); }
}



static void __attribute__((noinline))
called_by_test_5 (void)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();
  PyEval_RestoreThread(_save); }
}

void test_5 (PyObject *obj)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();
  called_by_test_5 ();
  PyEval_RestoreThread(_save); }
}



static void __attribute__((noinline))
called_by_test_6 (PyObject *obj)
{
  do { (((PyObject*)(obj)))->ob_refcnt++; } while (0);;
  do { if (--(((PyObject*)(obj)))->ob_refcnt == 0) { _Py_Dealloc(((PyObject*)(obj))); } } while (0);
}

void test_6 (PyObject *obj)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();
  called_by_test_6 (obj);
  PyEval_RestoreThread(_save); }
}

extern void called_by_test_7 (PyObject *obj);

void test_7 (PyObject *obj)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();
  called_by_test_7 (obj);
  PyEval_RestoreThread(_save); }
}

typedef void (*callback_t) (PyObject *);

void test_8 (PyObject *obj, callback_t cb)
{
  { PyThreadState *_save; _save = PyEval_SaveThread();
  cb (obj);
  PyEval_RestoreThread(_save); }
}
