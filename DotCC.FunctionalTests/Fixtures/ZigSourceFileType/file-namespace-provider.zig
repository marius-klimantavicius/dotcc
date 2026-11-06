// Static use of this file must not demand its instance fields.
value: @compileError("unused file instance layout"),
pub const answer = 39;
pub fn add(value: u8) u8 {
    return value + answer;
}
