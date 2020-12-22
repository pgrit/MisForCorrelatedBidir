using SeeSharp.Experiments;
using System.Collections.Generic;
using System.Globalization;
using MisForCorrelatedBidir.Common;

namespace MisForCorrelatedBidir.VcmExperiment {
    class Program {
        static void RunBench() {
            float resolutionScale = 2.0f;
            Benchmark bench = new(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
                {"ModernHall", new VcmExperiment(new ModernHall()) },
                {"BananaRange", new VcmExperiment(new BananaRange()) },
                {"ContemporaryBathroom", new VcmExperiment(new ContemporaryBathroom()) },
                {"HomeOffice", new VcmExperiment(new HomeOffice()) },
                {"RoughGlasses", new VcmExperiment(new RoughGlasses()) },
                {"RoughGlassesIndirect", new VcmExperiment(new RoughGlassesIndirect()) },
                {"IndirectRoom", new VcmExperiment(new IndirectRoom()) },
                {"LivingRoom", new VcmExperiment(new LivingRoom()) },
                {"MinimalistWhiteRoom", new VcmExperiment(new MinimalistWhiteRoom()) },
            }, (int)(640 * resolutionScale), (int)(480 * resolutionScale));
            bench.Run(forceReference: false);
        }

        static void RunFovExperiment() {
            float[] resolutions = new[] {
                0.25f,
                1.0f,
                3.0f,
            };
            foreach (float resolutionScale in resolutions) {
                string suffix = $"-{resolutionScale:0.000}";

                Benchmark bench = new(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
                    {"RoughGlassesIndirect-Narrow"+suffix, new VcmExperiment(new RoughGlassesIndirectNarrow(), true) },
                    {"RoughGlassesIndirect-Wide"+suffix, new VcmExperiment(new RoughGlassesIndirectWide(), true) },
                    {"RoughGlassesIndirect-Normal"+suffix, new VcmExperiment(new RoughGlassesIndirect(), true) },

                    {"RoughGlasses-Wide"+suffix, new VcmExperiment(new RoughGlassesWide(), true) },
                    {"RoughGlasses-Narrow"+suffix, new VcmExperiment(new RoughGlassesNarrow(), true) },
                    {"RoughGlasses-Normal"+suffix, new VcmExperiment(new RoughGlasses(), true) },

                    {"RoughGlasses-Lens"+suffix, new VcmExperiment(new RoughGlassesLens(), true)},
                    {"RoughGlassesIndirect-Lens"+suffix, new VcmExperiment(new RoughGlassesIndirectLens(), true)},
                }, (int)(640 * resolutionScale), (int)(480 * resolutionScale));
                bench.Run(forceReference: false);
            }
        }

        static void Main(string[] args) {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            RunBench();
            // RunFovExperiment();
        }
    }
}
