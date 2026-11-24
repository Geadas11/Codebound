// Pre-script for the 2nd headless run: skip Ghidra's PDB analyzer (its type layout step stalls on this PDB)
// and put the function names from our own pdb_proc_ranges.tsv (name, section, offset, size) as labels.
// shortcut: flat names, no PDB types/namespaces; good enough to read decompiled code by name.
import java.nio.file.*;
import ghidra.app.script.GhidraScript;
import ghidra.framework.options.Options;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Program;
import ghidra.program.model.symbol.*;

public class ApplyPdbNames extends GhidraScript {
	@Override
	public void run() throws Exception {
		Options o = currentProgram.getOptions(Program.ANALYSIS_PROPERTIES);
		o.setBoolean("PDB Universal", false);
		o.setBoolean("PDB MSDIA", false);
		o.setBoolean("Decompiler Parameter ID", false);

		Address text = currentProgram.getMemory().getBlock(".text").getStart();
		SymbolTable st = currentProgram.getSymbolTable();
		int n = 0, bad = 0;
		for (String line : Files.readAllLines(Path.of(getScriptArgs()[0]))) {
			String[] f = line.split("\t");
			if (f.length < 4 || !f[1].equals("1")) { bad++; continue; }
			String name = f[0].replaceAll("[\\s]", "_");
			try {
				st.createLabel(text.add(Long.parseLong(f[2])), name, SourceType.IMPORTED);
				n++;
			} catch (Exception e) { bad++; }
		}
		println("NAMES applied=" + n + " skipped=" + bad);
		if (n < 200000) throw new IllegalStateException("too few names applied: " + n);
	}
}
