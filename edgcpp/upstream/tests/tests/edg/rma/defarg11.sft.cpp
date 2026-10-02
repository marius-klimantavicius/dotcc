//options_all:-r -x -tused
//options: --strict;cp

#ifdef __APPLE__
typedef __EDG_SIZE_TYPE__ size_t;
#else
typedef __EDG_SIZE_TYPE__ size_t;
#endif
void* operator new(size_t, size_t=1);
class T {
        void* operator new(size_t, size_t=1);
};


