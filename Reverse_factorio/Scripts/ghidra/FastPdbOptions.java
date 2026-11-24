// Pre-script for the 2nd headless run: PDB names only (skip the slow type layout), no Decompiler Parameter ID.
// Run with -preScript FastPdbOptions.java [list] ; "list" only prints the current options.
import java.util.Map;
import ghidra.app.script.GhidraScript;
import ghidra.app.util.pdb.pdbapplicator.PdbApplicatorControl;
import ghidra.framework.options.Options;
import ghidra.program.model.listing.Program;

public class FastPdbOptions extends GhidraScript {
	@Override
	public void run() throws Exception {
		Map<String, String> cur = getCurrentAnalysisOptionsAndValues(currentProgram);
		String[] args = getScriptArgs();
		if (args.length == 0 || !args[0].equals("list")) {
			Options o = currentProgram.getOptions(Program.ANALYSIS_PROPERTIES);
			String ctl = cur.keySet().stream().filter(k -> k.startsWith("PDB Universal") && k.endsWith("Control")).findFirst()
				.orElseThrow(() -> new IllegalStateException("PDB Universal Control option not found"));
			o.setEnum(ctl, PdbApplicatorControl.PUBLIC_SYMBOLS_ONLY);
			o.setBoolean("Decompiler Parameter ID", false);
			cur = getCurrentAnalysisOptionsAndValues(currentProgram);
		}
		for (var e : cur.entrySet())
			if (e.getKey().startsWith("PDB") || e.getKey().startsWith("Decompiler Parameter ID"))
				println("OPT " + e.getKey() + " = " + e.getValue());
	}
}
