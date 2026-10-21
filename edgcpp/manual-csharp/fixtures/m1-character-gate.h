#ifndef M1_CHARACTER_GATE_H
#define M1_CHARACTER_GATE_H
#define RETURN(x) return x
#define CHAR_VALUE 'A'
#define TARGET_CHAR u'A'
template<class T> constexpr T id(T value){T local=value; RETURN(local);}
#endif
