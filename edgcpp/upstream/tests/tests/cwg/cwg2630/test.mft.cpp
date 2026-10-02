//type:fp
//options_all:--ms_c++latest -tused --modules --microsoft_version 1929 --ms_mod_file_map A.ifc --target win32
import A;
X x; // is X complete at this point?

//cwg: 2630
//title: Syntactic specification of class completeness
//meeting: Kona 11/22
//edg_status: Passes
