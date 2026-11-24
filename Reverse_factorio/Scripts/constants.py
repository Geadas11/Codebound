"""Dump named constants from factorio.pdb (read-only).
  S_CONSTANT (0x1107): compile-time integer/enum constants with their value in the PDB.
  S_GDATA32/S_LDATA32 (0x110D/0x110C) with a primitive numeric type in .rdata (section 2): value read from the exe.
Usage: python -I constants.py <factorio.pdb> <factorio.exe> <out.tsv>
"""
import struct, sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pdb_dump import MSF, TPI, cstr, numeric

pdb, exe_path, out = sys.argv[1:]
msf = MSF(pdb)
dbi = msf.stream(3)
symrec = struct.unpack_from("<H", dbi, 20)[0]  # DBI header: SymRecordStream
sym = msf.stream(symrec)
tpi = TPI(msf.stream(2))
exe = open(exe_path, "rb").read()
RDATA_RAW = 0x1f86e00  # .rdata PointerToRawData (artifacts/pe_info.json)
PRIM = {0x40: ("<f", "float"), 0x41: ("<d", "double"), 0x74: ("<i", "int32"), 0x75: ("<I", "uint32"),
        0x76: ("<q", "int64"), 0x77: ("<Q", "uint64"), 0x12: ("<i", "int32"), 0x22: ("<I", "uint32"),
        0x13: ("<q", "int64"), 0x23: ("<Q", "uint64"), 0x30: ("<?", "bool"), 0x20: ("<B", "uint8"),
        0x21: ("<H", "uint16"), 0x11: ("<h", "int16"), 0x72: ("<h", "int16"), 0x73: ("<H", "uint16")}
rows, p = set(), 0
while p + 4 <= len(sym):
    ln, k = struct.unpack_from("<HH", sym, p)
    b = sym[p + 4:p + 2 + ln]
    if k == 0x1107:
        ti = struct.unpack_from("<I", b, 0)[0]
        v, q = numeric(b, 4)
        while ti >= 0x1000 and tpi.rec(ti)[0] == 0x1001:  # LF_MODIFIER (const) -> underlying type
            ti = struct.unpack_from("<I", tpi.rec(ti)[1], 0)[0]
        if ti == 0x40:  # float constant stored as raw bits
            v = struct.unpack("<f", struct.pack("<I", v & 0xFFFFFFFF))[0]
        elif ti == 0x41:
            v = struct.unpack("<d", struct.pack("<Q", v & 0xFFFFFFFFFFFFFFFF))[0]
        rows.add(("const" if ti >= 0x1000 else PRIM.get(ti, ("", f"prim{ti:#x}"))[1], cstr(b, q)[0], repr(v)))
    elif k in (0x110C, 0x110D):
        ti, off, seg = struct.unpack_from("<IIH", b, 0)
        base = ti & 0xFFF  # primitive types only; pointer/modifier bits (0x400/0x600…) excluded below
        if ti < 0x1000 and (ti & 0xF00) == 0 and base in PRIM and seg == 2:
            fmt, tn = PRIM[base]
            v = struct.unpack_from(fmt, exe, RDATA_RAW + off)[0]
            rows.add((tn, cstr(b, 10)[0], repr(v)))
    p += 2 + ln
assert len(rows) > 1000, f"too few constants: {len(rows)}"
with open(out, "w", encoding="utf-8") as f:
    f.write("kind\tname\tvalue\n")
    for r in sorted(rows, key=lambda r: r[1]):
        f.write("\t".join(r) + "\n")
print(len(rows), "constants ->", out)
