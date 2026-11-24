"""Direct-call extractor (read-only): lists `call rel32` (E8) and tail `jmp rel32` (E9)
targets inside a function, in address order, using exact function ranges from the PDB.

Usage: python -I calls.py <factorio.pdb> <factorio.exe> <out_dir> "Func::name" [...]
First run writes <out_dir>/pdb_proc_ranges.tsv (name, seg, offset, length) from S_GPROC32/S_LPROC32.

shortcut: byte scan, not a disassembler: an E8/E9 byte inside another instruction can yield a false
target; only targets landing exactly on a known function start are kept, which removes almost all noise.
Indirect/virtual calls (call [reg+x]) are not visible. Upgrade to Ghidra/REA when its analysis is ready.
"""
import mmap, os, struct, sys
sys.path.insert(0, os.path.dirname(__file__))
from pdb_dump import MSF, cstr


def proc_ranges(msf):
    dbi = msf.stream(3)
    modsz = struct.unpack_from("<i", dbi, 24)[0]
    p, out = 64, []
    while p < 64 + modsz:
        symstream, symbytes = struct.unpack_from("<HI", dbi, p + 34)
        _, q = cstr(dbi, p + 64)
        _, q = cstr(dbi, q)
        p = (q + 3) & ~3
        if symstream == 0xFFFF or symbytes <= 4:
            continue
        s = msf.stream(symstream)[:symbytes]
        r = 4  # CV_SIGNATURE_C13
        while r + 4 <= len(s):
            ln, k = struct.unpack_from("<HH", s, r)
            if k in (0x1110, 0x110F):  # S_GPROC32 / S_LPROC32
                length = struct.unpack_from("<I", s, r + 16)[0]
                off, seg = struct.unpack_from("<IH", s, r + 32)
                out.append((cstr(s, r + 39)[0], seg, off, length))
            r += 2 + ln
    return out


def main():
    pdb, exe, outdir, *names = sys.argv[1:]
    cache = os.path.join(outdir, "pdb_proc_ranges.tsv")
    if not os.path.exists(cache):
        rows = proc_ranges(MSF(pdb))
        assert len(rows) > 100000, "too few procedures: S_PROC32 layout wrong?"
        with open(cache, "w", encoding="utf-8") as f:
            f.writelines(f"{n}\t{s}\t{o}\t{l}\n" for n, s, o, l in rows)
    rows = [l.rstrip("\n").split("\t") for l in open(cache, encoding="utf-8")]
    rows = [(n, int(s), int(o), int(l)) for n, s, o, l in rows]
    text = [r for r in rows if r[1] == 1]  # seg 1 = .text (RVA 0x1000, file 0x400)
    start = {o: n for n, _s, o, _l in text}
    byname = {}
    for n, _s, o, l in text:
        byname.setdefault(n, (o, l))
    f = open(exe, "rb")
    m = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
    for name in names:
        if name not in byname:
            print(f"### {name}: not found")
            continue
        o, l = byname[name]
        code = m[0x400 + o:0x400 + o + l]
        print(f"### {name}  rva={0x1000 + o:#x} len={l}")
        seen = []
        for i in range(len(code) - 4):
            if code[i] in (0xE8, 0xE9):
                tgt = o + i + 5 + struct.unpack_from("<i", code, i + 1)[0]
                if tgt in start and tgt != o:
                    seen.append(("call" if code[i] == 0xE8 else "jmp ") + f" +{i:#06x} {start[tgt]}")
        print("\n".join(seen) or "(no direct calls)")


if __name__ == "__main__":
    main()
