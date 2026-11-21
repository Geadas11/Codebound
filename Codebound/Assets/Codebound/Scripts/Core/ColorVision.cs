namespace Codebound.Core
{
    /// <summary>
    /// Colour-blind screen filter: copies the colour difference a viewer cannot see into a channel they can.
    /// Red/green deficiencies: blue += 0.7·(red − green), so red and green things also differ in blue.
    /// Tritanopia (blue/yellow): red += 0.35·(blue − green), green −= 0.35·(blue − green).
    /// Applied to the whole screen as a channel mixer (Game/ColorFilter). The simulation matrices are only used to test it.
    /// </summary>
    public static class ColorVision
    {
        public static readonly string[] Modes = { "off", "deuteranopia", "protanopia", "tritanopia" };

        static readonly float[][] Simulate =
        {
            new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            new float[] { 0.625f, 0.375f, 0, 0.7f, 0.3f, 0, 0, 0.3f, 0.7f },       // deuteranopia (no green cones)
            new float[] { 0.567f, 0.433f, 0, 0.558f, 0.442f, 0, 0, 0.242f, 0.758f }, // protanopia (no red cones)
            new float[] { 0.95f, 0.05f, 0, 0, 0.433f, 0.567f, 0, 0.475f, 0.525f },  // tritanopia (no blue cones)
        };

        static readonly float[][] Corrections =
        {
            new float[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            new float[] { 1, 0, 0, 0, 1, 0, 0.7f, -0.7f, 1 },
            new float[] { 1, 0, 0, 0, 1, 0, 0.7f, -0.7f, 1 },
            new float[] { 1, -0.35f, 0.35f, 0, 1.35f, -0.35f, 0, 0, 1 },
        };

        /// <summary>Row-major 3×3 matrix: out.r = m[0]·r + m[1]·g + m[2]·b, etc.</summary>
        public static float[] Correction(int mode) => (float[])Corrections[System.Math.Max(0, System.Math.Min(Corrections.Length - 1, mode))].Clone();

        public static (float r, float g, float b) Apply(float[] m, float r, float g, float b) =>
            (m[0] * r + m[1] * g + m[2] * b, m[3] * r + m[4] * g + m[5] * b, m[6] * r + m[7] * g + m[8] * b);

        public static (float r, float g, float b) SimulateDeficiency(int mode, float r, float g, float b) => Apply(Simulate[mode], r, g, b);
    }
}
