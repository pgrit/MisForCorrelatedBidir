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
                {"TargetPractice", new VcmExperiment(new TargetPractice()) },
                {"ContemporaryBathroom", new VcmExperiment(new ContemporaryBathroom()) },
                {"HomeOffice", new VcmExperiment(new HomeOffice()) },
                {"RoughGlasses", new VcmExperiment(new RoughGlasses()) },
                {"RoughGlassesIndirect", new VcmExperiment(new RoughGlassesIndirect()) },
                {"IndirectRoom", new VcmExperiment(new IndirectRoom()) },
                {"LivingRoom", new VcmExperiment(new LivingRoom()) },
                {"MinimalistWhiteRoom", new VcmExperiment(new MinimalistWhiteRoom()) },
                {"LampCausticNoShade", new VcmExperiment(new LampCausticNoShade()) },
                {"LampCaustic", new VcmExperiment(new LampCaustic()) },
            }, (int)(640 * resolutionScale), (int)(480 * resolutionScale));
            bench.Run(forceReference: false);
        }

        static void RunFovExperiment() {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

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

        static void RunRadiusExperiment() {
            float resolutionScale = 2.0f;
            Benchmark bench = new(new Dictionary<string, SeeSharp.Experiments.ExperimentFactory>(){
                {"ModernHall", new RadiusExperiment(new ModernHall()) },
                {"TargetPractice", new RadiusExperiment(new TargetPractice()) },
                {"ContemporaryBathroom", new RadiusExperiment(new ContemporaryBathroom()) },
                {"HomeOffice", new RadiusExperiment(new HomeOffice()) },
                {"RoughGlasses", new RadiusExperiment(new RoughGlasses()) },
                {"RoughGlassesIndirect", new RadiusExperiment(new RoughGlassesIndirect()) },
                {"IndirectRoom", new RadiusExperiment(new IndirectRoom()) },
                {"LivingRoom", new RadiusExperiment(new LivingRoom()) },
                {"MinimalistWhiteRoom", new RadiusExperiment(new MinimalistWhiteRoom()) },
                {"LampCausticNoShade", new RadiusExperiment(new LampCausticNoShade()) },
                {"LampCaustic", new RadiusExperiment(new LampCaustic()) },
            }, (int)(640 * resolutionScale), (int)(480 * resolutionScale));
            bench.Run(forceReference: false);
        }

        static void Main(string[] args) {
            RunBench();
            // RunRadiusExperiment();
            // RunFovExperiment();
        }
    }
}
