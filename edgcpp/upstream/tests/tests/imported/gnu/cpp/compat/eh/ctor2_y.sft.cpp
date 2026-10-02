//type: fp
//options: 
# 0 "./compat/eh/ctor2_y.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/eh/ctor2_y.C"
extern int r;
void *p;

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
# 5 "./compat/eh/ctor2_y.C" 2

VBase::VBase ()
{
  p = this;
}

VBase::~VBase ()
{
  if (p != this) r = 1;
}

Stream::Stream () {}
DerivedStream::DerivedStream ()
{
  throw 1;
}
