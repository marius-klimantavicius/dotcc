//type: fp
//options: 
# 0 "./ext/dllexport-MI1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/dllexport-MI1.C"
# 10 "./ext/dllexport-MI1.C"
# 1 "./ext/dll-MI1.h" 1
# 13 "./ext/dll-MI1.h"
class __attribute__ ((dllexport)) MBase
{
public:
  virtual int vf() const = 0;
  virtual ~MBase();
};

class __attribute__ ((dllexport)) D1 : virtual public MBase
{
public:
  int vf() const;
};

class __attribute__ ((dllexport)) D2 : virtual public MBase
{
public:
  D2 ();
  D2 (D2 const&);
  int vf() const;
};

class __attribute__ ((dllexport)) MI1 : public D1, public D2
{
public:
  int vf() const;
};
# 11 "./ext/dllexport-MI1.C" 2

MBase::~MBase(){}

int D1::vf() const { return 1; }

D2::D2() { }
D2::D2 (D2 const&) { }
int D2::vf() const { return 2; }

int MI1::vf() const { return D1::vf();}


__attribute__ ((dllexport)) MI1 dllMI1;


__attribute__ ((dllexport)) MI1 dllMI1Copy = dllMI1;
