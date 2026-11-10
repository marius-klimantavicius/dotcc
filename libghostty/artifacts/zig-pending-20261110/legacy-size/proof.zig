const std=@import("std");
const original=@import("terminal/original.zig");
const patched=@import("terminal/patched.zig");
const options=@import("terminal_options");
const native_receipt=@import("proof_options").native_receipt;
const expected_hash:u64=3517276540470399028;
fn appendHash(hash:*u64,byte:u8) void {hash.*=(hash.* ^ byte) *% 1099511628211;}
const names=[_] []const u8{"rows","columns","cell_width","cell_height"};
const Values=struct{rows:u16,columns:u16,cell_width:u32,cell_height:u32};
const cases=[_]Values{
 .{.rows=0,.columns=0,.cell_width=0,.cell_height=0},
 .{.rows=24,.columns=80,.cell_width=9,.cell_height=18},
 .{.rows=1,.columns=1,.cell_width=1,.cell_height=1},
 .{.rows=65535,.columns=65535,.cell_width=4294967295,.cell_height=4294967295},
 .{.rows=65535,.columns=1,.cell_width=1,.cell_height=4294967295},
 .{.rows=1,.columns=65535,.cell_width=4294967295,.cell_height=1},
 .{.rows=32768,.columns=32767,.cell_width=65535,.cell_height=65536},
 .{.rows=17,.columns=257,.cell_width=99999,.cell_height=100000},
 .{.rows=65535,.columns=0,.cell_width=4294967295,.cell_height=0},
};
const styles=[_]original.Style{.mode_2048,.csi_14_t,.csi_16_t,.csi_18_t,.csi_19_t};
const sample_expected=[_] []const u8{"\x1b[48;24;80;432;720t","\x1b[4;432;720t","\x1b[6;18;9t","\x1b[8;24;80t","\x1b[9;24;80t"};
const max_expected=[_] []const u8{"\x1b[48;65535;65535;281470681677825;281470681677825t","\x1b[4;281470681677825;281470681677825t","\x1b[6;4294967295;4294967295t","\x1b[8;65535;65535t","\x1b[9;65535;65535t"};
pub fn main() u8 { return answer(); }
comptime { @export(&answer, .{ .name="answer" }); }
pub fn answer() callconv(.c) u8 {
 if(@sizeOf(original.Size)!=@sizeOf(patched.Size) or @alignOf(original.Size)!=@alignOf(patched.Size))return 1;
 if(comptime @typeInfo(original.Size).@"struct".layout!=@typeInfo(patched.Size).@"struct".layout or @typeInfo(original.Size).@"struct".layout!=(if(options.c_abi) .@"extern" else .auto))return 2;
 if(@typeInfo(original.Size).@"struct".field_names.len!=4 or @typeInfo(patched.Size).@"struct".field_names.len!=4)return 3;
 {
  if(!std.mem.eql(u8,@typeInfo(original.Size).@"struct".field_names[0],"rows") or !std.mem.eql(u8,@typeInfo(patched.Size).@"struct".field_names[0],"rows"))return 4;
  if(@FieldType(original.Size,"rows")!=@FieldType(patched.Size,"rows"))return 5;
  if(@offsetOf(original.Size,"rows")!=@offsetOf(patched.Size,"rows"))return 6;
  if(@sizeOf(@FieldType(original.Size,"rows"))!=@sizeOf(@FieldType(patched.Size,"rows")) or @alignOf(@FieldType(original.Size,"rows"))!=@alignOf(@FieldType(patched.Size,"rows")))return 7;
  } {
  if(!std.mem.eql(u8,@typeInfo(original.Size).@"struct".field_names[1],"columns") or !std.mem.eql(u8,@typeInfo(patched.Size).@"struct".field_names[1],"columns"))return 4;
  if(@FieldType(original.Size,"columns")!=@FieldType(patched.Size,"columns"))return 5;
  if(@offsetOf(original.Size,"columns")!=@offsetOf(patched.Size,"columns"))return 6;
  if(@sizeOf(@FieldType(original.Size,"columns"))!=@sizeOf(@FieldType(patched.Size,"columns")) or @alignOf(@FieldType(original.Size,"columns"))!=@alignOf(@FieldType(patched.Size,"columns")))return 7;
  } {
  if(!std.mem.eql(u8,@typeInfo(original.Size).@"struct".field_names[2],"cell_width") or !std.mem.eql(u8,@typeInfo(patched.Size).@"struct".field_names[2],"cell_width"))return 4;
  if(@FieldType(original.Size,"cell_width")!=@FieldType(patched.Size,"cell_width"))return 5;
  if(@offsetOf(original.Size,"cell_width")!=@offsetOf(patched.Size,"cell_width"))return 6;
  if(@sizeOf(@FieldType(original.Size,"cell_width"))!=@sizeOf(@FieldType(patched.Size,"cell_width")) or @alignOf(@FieldType(original.Size,"cell_width"))!=@alignOf(@FieldType(patched.Size,"cell_width")))return 7;
  } {
  if(!std.mem.eql(u8,@typeInfo(original.Size).@"struct".field_names[3],"cell_height") or !std.mem.eql(u8,@typeInfo(patched.Size).@"struct".field_names[3],"cell_height"))return 4;
  if(@FieldType(original.Size,"cell_height")!=@FieldType(patched.Size,"cell_height"))return 5;
  if(@offsetOf(original.Size,"cell_height")!=@offsetOf(patched.Size,"cell_height"))return 6;
  if(@sizeOf(@FieldType(original.Size,"cell_height"))!=@sizeOf(@FieldType(patched.Size,"cell_height")) or @alignOf(@FieldType(original.Size,"cell_height"))!=@alignOf(@FieldType(patched.Size,"cell_height")))return 7;
  }
 if(@typeInfo(original.Style).@"enum".tag_type!=@typeInfo(patched.Style).@"enum".tag_type)return 8;
 {
  if(@backingInt(original.Style.mode_2048)!=0 or @backingInt(@field(patched.Style,@tagName(original.Style.mode_2048)))!=0)return 9;
  } {
  if(@backingInt(original.Style.csi_14_t)!=1 or @backingInt(@field(patched.Style,@tagName(original.Style.csi_14_t)))!=1)return 9;
  } {
  if(@backingInt(original.Style.csi_16_t)!=2 or @backingInt(@field(patched.Style,@tagName(original.Style.csi_16_t)))!=2)return 9;
  } {
  if(@backingInt(original.Style.csi_18_t)!=3 or @backingInt(@field(patched.Style,@tagName(original.Style.csi_18_t)))!=3)return 9;
  } {
  if(@backingInt(original.Style.csi_19_t)!=4 or @backingInt(@field(patched.Style,@tagName(original.Style.csi_19_t)))!=4)return 9;
  }
 var comparisons:usize=0;
 var hash:u64=14695981039346656037;
 const expected_offsets=if(options.c_abi) [_]usize{0,2,4,8} else [_]usize{8,10,0,4};
 if(@sizeOf(patched.Size)!=12 or @alignOf(patched.Size)!=4)return 14;
 if(@offsetOf(patched.Size,"rows")!=expected_offsets[0])return 15; if(@offsetOf(patched.Size,"columns")!=expected_offsets[1])return 15; if(@offsetOf(patched.Size,"cell_width")!=expected_offsets[2])return 15; if(@offsetOf(patched.Size,"cell_height")!=expected_offsets[3])return 15;
 for(cases,0..) |value,case_index| {
  const a:original.Size=.{.rows=value.rows,.columns=value.columns,.cell_width=value.cell_width,.cell_height=value.cell_height};
  const b:patched.Size=.{.rows=value.rows,.columns=value.columns,.cell_width=value.cell_width,.cell_height=value.cell_height};
  for(styles,0..) |style,style_index| {
   const target_style:patched.Style=@enumFromInt(@backingInt(style));
   for(0..65) |capacity| {
    var ab:[66]u8=@splat(0xa5);var bb:[66]u8=@splat(0xa5);
    var aw=std.Io.Writer.fixed(ab[1..1+capacity]);var bw=std.Io.Writer.fixed(bb[1..1+capacity]);
    const ae:u16=if(original.encode(&aw,style,a)) |_| 0 else |err| if(err==error.WriteFailed) 1 else 2;
    const be:u16=if(patched.encode(&bw,target_style,b)) |_| 0 else |err| if(err==error.WriteFailed) 1 else 2;
    if(ae!=be or !std.mem.eql(u8,aw.buffered(),bw.buffered()))return 10;
    if(!std.mem.eql(u8,&ab,&bb) or ab[0]!=0xa5 or ab[1+capacity]!=0xa5)return 11;
    if(case_index==1 or case_index==3) {
     const expected=if(case_index==1) sample_expected[style_index] else max_expected[style_index];
     if((capacity<expected.len)!=(ae!=0) or aw.buffered().len!=@min(capacity,expected.len))return 12;
     if(!std.mem.eql(u8,aw.buffered(),expected[0..aw.buffered().len]))return 13;
    }
    appendHash(&hash,if(ae==0) 0 else 1);
    appendHash(&hash,@intCast(aw.buffered().len));
    for(ab) |byte| appendHash(&hash,byte);
    comparisons+=1;
   }
  }
 }
 if(comparisons!=2925)return 16;
 if(comptime native_receipt) {
  std.debug.print("LAYOUT c_abi={} size={d} align={d} offsets={d},{d},{d},{d} comparisons={d} hash={d}\n",.{options.c_abi,@sizeOf(patched.Size),@alignOf(patched.Size),@offsetOf(patched.Size,"rows"),@offsetOf(patched.Size,"columns"),@offsetOf(patched.Size,"cell_width"),@offsetOf(patched.Size,"cell_height"),comparisons,hash});
 } else if(hash!=expected_hash)return 17;
 return 42;
}
