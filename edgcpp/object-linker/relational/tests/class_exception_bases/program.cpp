#include "program.h"
int public_indirect() {
    PublicIndirect source;
    source.value = 17;
    try { throw source; }
    catch (Root& value) { return value.value == 17 ? 0 : 1; }
    catch (...) { return 2; }
}
int private_base() {
    PrivateBase source;
    source.set(19);
    try { throw source; }
    catch (Root&) { return 1; }
    catch (PrivateBase& value) { return value.get() == 19 ? 0 : 2; }
    catch (...) { return 3; }
}
int ambiguous_base() {
    Ambiguous source;
    source.BranchOne::value = 23;
    source.BranchTwo::value = 29;
    try { throw source; }
    catch (Root&) { return 1; }
    catch (Ambiguous& value) {
        return value.BranchOne::value == 23 && value.BranchTwo::value == 29 ? 0 : 2;
    }
    catch (...) { return 3; }
}
int virtual_diamond(const Diamond& source, size_type expected_offset) {
    try { throw source; }
    catch (Diamond& complete) {
        try { throw; }
        catch (Shared& value) {
            auto offset = reinterpret_cast<unsigned char*>(&value) - reinterpret_cast<unsigned char*>(&complete);
            return value.value == 31 && static_cast<size_type>(offset) == expected_offset ? 0 : 1;
        }
        catch (...) { return 2; }
    }
    catch (...) { return 3; }
}
