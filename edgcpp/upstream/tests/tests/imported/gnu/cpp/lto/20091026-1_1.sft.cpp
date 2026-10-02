//type: fp
//options: 
# 0 "./lto/20091026-1_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091026-1_1.C"
# 1 "./lto/20091026-1_a.h" 1
class cObject {
public:
    cObject *firstchildp;
};
class cHead : public cObject {
public:
    cObject *find(const char *objname) const;
};
# 2 "./lto/20091026-1_1.C" 2

#pragma GCC diagnostic ignored "-Wreturn-type"
extern cHead networks;
class cNetworkType;
inline cNetworkType *findNetwork(const char *s)
{
  return (cNetworkType *)networks.find(s);
}
int run(const char *opt_network_name)
{
  cNetworkType *network = findNetwork(opt_network_name);
  if (!network)
    throw 1;
}
