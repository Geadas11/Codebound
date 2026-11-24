// Post-script (read-only use): decompile every function whose label matches a line of the names file.
// Args: <names file> <output dir>. Writes <out>/<safe name>@<address>.c and prints a status line per name.
import java.nio.file.*;
import ghidra.app.decompiler.*;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.Symbol;

public class DecompileList extends GhidraScript {
	@Override
	public void run() throws Exception {
		String[] a = getScriptArgs();
		Path out = Path.of(a[1]);
		Files.createDirectories(out);
		DecompInterface d = new DecompInterface();
		d.openProgram(currentProgram);
		int ok = 0, miss = 0;
		for (String name : Files.readAllLines(Path.of(a[0]))) {
			if (name.isBlank()) continue;
			var syms = currentProgram.getSymbolTable().getGlobalSymbols(name.trim());
			if (syms.isEmpty()) { println("MISS " + name); miss++; continue; }
			for (Symbol s : syms) {
				Function f = getFunctionAt(s.getAddress());
				if (f == null) f = createFunction(s.getAddress(), null);
				if (f == null) { println("NOFUNC " + name + " @" + s.getAddress()); miss++; continue; }
				DecompileResults r = d.decompileFunction(f, 180, monitor);
				String c = r.decompileCompleted() ? r.getDecompiledFunction().getC() : "// FAILED: " + r.getErrorMessage();
				String file = name.replaceAll("[^A-Za-z0-9_.-]", "_");
				if (file.length() > 120) file = file.substring(0, 120);
				Files.writeString(out.resolve(file + "@" + s.getAddress() + ".c"),
					"// " + name + " @ " + s.getAddress() + " (function " + f.getName() + ")\n" + c);
				println((r.decompileCompleted() ? "OK " : "FAIL ") + name + " @" + s.getAddress());
				ok++;
			}
		}
		d.dispose();
		println("DECOMP done ok=" + ok + " miss=" + miss);
	}
}
