#ifndef SEMANTIC_API_H
#define SEMANTIC_API_H
#define API_VERSION (2 + 3)
#define ADD_API(x, y) ((x) + (y))
#define OUTER(x) ADD_API(x, API_VERSION)
#endif
