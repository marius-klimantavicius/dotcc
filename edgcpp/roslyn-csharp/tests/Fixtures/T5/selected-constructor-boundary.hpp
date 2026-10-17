#ifndef SELECTED_CONSTRUCTOR_BOUNDARY_HPP
#define SELECTED_CONSTRUCTOR_BOUNDARY_HPP

struct BoundaryCell {
    long long canary;
    int serial;
    unsigned char kind;

    BoundaryCell();
};

extern BoundaryCell first;
extern BoundaryCell second;

#endif
