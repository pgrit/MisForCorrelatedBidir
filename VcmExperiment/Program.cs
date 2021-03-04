using SeeSharp.Experiments;
using System.Globalization;
using SeeSharp.Image;
using System.Diagnostics;

namespace MisForCorrelatedBidir.VcmExperiment {
    class Program {
        static void RunBench() {
            SceneRegistry.AddSource("../Scenes");

            float resolutionScale = 2.0f;
            Benchmark bench = new(new VcmExperiment(), new() {
                SceneRegistry.LoadScene("ModernHall"),
                SceneRegistry.LoadScene("LivingRoom"),
                SceneRegistry.LoadScene("TargetPractice"),
                SceneRegistry.LoadScene("HomeOffice"),
                SceneRegistry.LoadScene("RoughGlasses", maxDepth: 10),
                SceneRegistry.LoadScene("RoughGlassesIndirect", maxDepth: 10),
                SceneRegistry.LoadScene("IndirectRoom"),
                SceneRegistry.LoadScene("MinimalistWhiteRoom"),
                SceneRegistry.LoadScene("LampCaustic", maxDepth: 10),
            }, "Results",
            (int)(640 * resolutionScale), (int)(480 * resolutionScale),
            FrameBuffer.Flags.SendToTev);

            bench.Run();

            Process.Start("python", "./makefigures.py").WaitForExit();
        }

        static void RunFovExperiment() {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            SceneRegistry.AddSource("../Scenes");

            float[] resolutions = new[] {
                0.25f,
                1.0f,
                3.0f,
            };

            foreach (float resolutionScale in resolutions) {

                Benchmark bench = new(new VcmExperiment(true), new() {
                    SceneRegistry.LoadScene("RoughGlassesIndirect", "Narrow", maxDepth: 10),
                    SceneRegistry.LoadScene("RoughGlassesIndirect", "Wide", maxDepth: 10),
                    SceneRegistry.LoadScene("RoughGlassesIndirect", maxDepth: 10),

                    SceneRegistry.LoadScene("RoughGlasses", "Narrow", maxDepth: 10),
                    SceneRegistry.LoadScene("RoughGlasses", "Wide", maxDepth: 10),
                    SceneRegistry.LoadScene("RoughGlasses", maxDepth: 10),

                    SceneRegistry.LoadScene("RoughGlassesIndirect", "Lens", maxDepth: 10),
                    SceneRegistry.LoadScene("RoughGlassesIndirect", "Lens", maxDepth: 10),
                },
                $"Results-{resolutionScale:0.000}",
                (int)(640 * resolutionScale), (int)(480 * resolutionScale),
                FrameBuffer.Flags.SendToTev);

                bench.Run();
            }
        }

        static void RunRadiusExperiment() {
            SceneRegistry.AddSource("../Scenes");
            float resolutionScale = 2.0f;
            Benchmark bench = new(new RadiusExperiment(), new() {
                SceneRegistry.LoadScene("ModernHall"),
                SceneRegistry.LoadScene("LivingRoom"),
                SceneRegistry.LoadScene("TargetPractice"),
                SceneRegistry.LoadScene("HomeOffice"),
                SceneRegistry.LoadScene("RoughGlasses", maxDepth: 10),
                SceneRegistry.LoadScene("RoughGlassesIndirect", maxDepth: 10),
                SceneRegistry.LoadScene("IndirectRoom"),
                SceneRegistry.LoadScene("MinimalistWhiteRoom"),
                SceneRegistry.LoadScene("LampCaustic", maxDepth: 10),
            }, "Results",
            (int)(640 * resolutionScale), (int)(480 * resolutionScale),
            FrameBuffer.Flags.SendToTev);

            bench.Run();

            Process.Start("python", "./radiusfigure.py").WaitForExit();
        }

        static void Main(string[] args) {
            RunBench();
            RunRadiusExperiment();
            // RunFovExperiment();
        }
    }
}
