//type: fp
//options: 
# 0 "./compat/eh/ctor2_x.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/ctor2_x.C"
extern "C" void exit (int);
extern "C" void abort (void);

# 1 "./compat/eh/ctor2.h" 1
struct VBase
{
  virtual void f () {}
  VBase();
  ~VBase();
};

struct StreamBase
{
  virtual ~StreamBase() {}
};

struct Stream : public virtual VBase, public StreamBase
{
  Stream();
  virtual ~Stream() {}
};

struct DerivedStream : public Stream
{
  DerivedStream();
};
# 5 "./compat/eh/ctor2_x.C" 2

int r;

void ctor2_x () {

  try
    {
      DerivedStream str;
    }
  catch (...) { }

  if (r != 0)
    abort ();
  exit (0);
}
