//options_all:-r -x -tused
//options: --strict;cp

// Problem in -b mode
template <class T>
const T*
bin_loc_r(
        int (*rel_ptr)(const T*, const T*),
        const T& value,
        const T* begin,
        const T* end
);

