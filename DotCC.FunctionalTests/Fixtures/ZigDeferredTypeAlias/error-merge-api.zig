pub const API = struct {
    pub const First = error{One};
    pub const Combined = First || error{Two};
};
