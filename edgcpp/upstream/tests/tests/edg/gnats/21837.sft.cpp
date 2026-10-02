//type:fn
//options_all:--microsoft_v 1924 --no_ms_permissive
int *f(bool *p) {
              p = false; // error: '=': cannot convert from 'bool' to 'bool *'
              p = 0; // OK
 
              return false; // error: 'return': cannot convert from 'bool' to 'int *'
}
