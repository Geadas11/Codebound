"""List the functions compiled into each Lua module (lua.lib) of factorio.pdb, with byte size (read-only).
Usage: python -I lua_funcs.py <factorio.pdb> <pdb_modules.tsv> <out.tsv>
"""
import struct, sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from pdb_dump import MSF, cstr

pdb, mods, out = sys.argv[1:]
msf = MSF(pdb)
rows = []
for line in open(mods, encoding="utf-8"):
    f = line.rstrip("\n").split("\t")
    if len(f) < 5 or not f[2].endswith("lua.lib"):
        continue
    obj = f[1].rsplit("\\", 1)[-1]
    s = msf.stream(int(f[3]))[:int(f[4])]
    p = 4  # module symbol stream starts with a 4-byte signature
    while p + 4 <= len(s):
        ln, k = struct.unpack_from("<HH", s, p)
        if k in (0x1110, 0x110F):  # S_GPROC32 / S_LPROC32: parent,end,next,len,dbgstart,dbgend,typind,off,seg,flags,name
            size = struct.unpack_from("<I", s, p + 4 + 12)[0]
            rows.append((obj, cstr(s, p + 4 + 35)[0], size, "global" if k == 0x1110 else "static"))
        p += 2 + ln
assert len(rows) > 500, len(rows)
with open(out, "w", encoding="utf-8") as fo:
    fo.write("module\tfunction\tbytes\tlinkage\n")
    for r in rows:
        fo.write("\t".join(map(str, r)) + "\n")
print(len(rows), "functions ->", out)
