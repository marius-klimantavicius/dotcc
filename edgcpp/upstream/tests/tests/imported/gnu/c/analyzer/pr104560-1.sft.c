//type: fp
//options: 
# 0 "./analyzer/pr104560-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/pr104560-1.c"



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 5 "./analyzer/pr104560-1.c" 2
# 18 "./analyzer/pr104560-1.c"

# 18 "./analyzer/pr104560-1.c"
struct ibv_device {

};

struct verbs_device {
 struct ibv_device device;

 int placeholder;
};

struct mlx5_device {
 struct verbs_device verbs_dev;
 int placeholder;
};

static inline struct mlx5_device *to_mdev(struct ibv_device *ibdev)
{
 return ((struct mlx5_device *) ((char *)(ibdev) - 
# 35 "./analyzer/pr104560-1.c" 3 4
       __builtin_offsetof (
# 35 "./analyzer/pr104560-1.c"
       struct mlx5_device
# 35 "./analyzer/pr104560-1.c" 3 4
       , 
# 35 "./analyzer/pr104560-1.c"
       verbs_dev.device
# 35 "./analyzer/pr104560-1.c" 3 4
       )
# 35 "./analyzer/pr104560-1.c"
       ) + ((typeof(*(ibdev)) *)0 != (typeof(((struct mlx5_device *)0)->verbs_dev.device) *)0));
}

static void mlx5_uninit_device(struct verbs_device *verbs_device)
{
        struct mlx5_device *dev = to_mdev(&verbs_device->device);

        __builtin_free(dev);
}
