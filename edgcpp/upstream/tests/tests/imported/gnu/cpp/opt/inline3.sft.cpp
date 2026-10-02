//type: fp
//options: 
# 0 "./opt/inline3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/inline3.C"





# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 7 "./opt/inline3.C" 2


# 8 "./opt/inline3.C"
typedef long unsigned int size_t;

template < class Type > class VectorNd
{
  size_t size;
  Type *data;
 public:

  VectorNd (size_t _size, size_t count, ...)
 : size (_size)
  {
    data = new Type[size];

    va_list ap;

    
# 23 "./opt/inline3.C" 3 4
   __builtin_va_start(
# 23 "./opt/inline3.C"
   ap
# 23 "./opt/inline3.C" 3 4
   ,
# 23 "./opt/inline3.C"
   count
# 23 "./opt/inline3.C" 3 4
   )
# 23 "./opt/inline3.C"
                       ;

    for (size_t i = 0; i < count; i++)
      data[i] = 
# 26 "./opt/inline3.C" 3 4
               __builtin_va_arg(
# 26 "./opt/inline3.C"
               ap
# 26 "./opt/inline3.C" 3 4
               ,
# 26 "./opt/inline3.C"
               Type
# 26 "./opt/inline3.C" 3 4
               )
# 26 "./opt/inline3.C"
                                ;

    
# 28 "./opt/inline3.C" 3 4
   __builtin_va_end(
# 28 "./opt/inline3.C"
   ap
# 28 "./opt/inline3.C" 3 4
   )
# 28 "./opt/inline3.C"
              ;
  }

  ~VectorNd ()
  {
    delete [] data;
  }
};

int main ()
{
  VectorNd <double> vector (3, 3, 1.0, 2.0, 3.0);
}
