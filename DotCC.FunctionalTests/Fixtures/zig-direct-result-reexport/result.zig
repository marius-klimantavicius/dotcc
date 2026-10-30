pub const Result = enum(c_int) {
    success = 0,
    out_of_memory = -1,
    invalid_value = -2,
};

pub fn result(valid: bool) Result {
    return if (valid) .success else .invalid_value;
}
