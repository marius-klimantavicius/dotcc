//type: fp
//options: 
# 0 "./lto/20091026-1_0.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20091026-1_0.C"




# 1 "./lto/20091026-1_a.h" 1
class cObject {
public:
    cObject *firstchildp;
};
class cHead : public cObject {
public:
    cObject *find(const char *objname) const;
};
# 6 "./lto/20091026-1_0.C" 2
cObject *cHead::find(const char *objname) const
{
    return firstchildp;
}
class cNetworkType : public cObject { };
cNetworkType *networktype;
