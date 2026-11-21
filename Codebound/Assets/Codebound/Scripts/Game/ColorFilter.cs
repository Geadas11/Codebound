using Codebound.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Codebound.Game
{
    /// <summary>Whole-screen colour-blind correction (Options → Accessibility), as a URP channel mixer on a global volume.</summary>
    public sealed class ColorFilter : MonoBehaviour
    {
        Volume volume;
        ChannelMixer mixer;
        int applied = -1;

        public void Init(Camera cam)
        {
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 50;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            mixer = profile.Add<ChannelMixer>(true);
            volume.sharedProfile = profile;
            Apply();
        }

        void Update() { if (applied != Settings.Version) Apply(); }

        void Apply()
        {
            applied = Settings.Version;
            int mode = Settings.ColorFilter;
            volume.weight = mode == 0 ? 0f : 1f;
            var m = ColorVision.Correction(mode);
            mixer.redOutRedIn.Override(m[0] * 100); mixer.redOutGreenIn.Override(m[1] * 100); mixer.redOutBlueIn.Override(m[2] * 100);
            mixer.greenOutRedIn.Override(m[3] * 100); mixer.greenOutGreenIn.Override(m[4] * 100); mixer.greenOutBlueIn.Override(m[5] * 100);
            mixer.blueOutRedIn.Override(m[6] * 100); mixer.blueOutGreenIn.Override(m[7] * 100); mixer.blueOutBlueIn.Override(m[8] * 100);
        }
    }
}
