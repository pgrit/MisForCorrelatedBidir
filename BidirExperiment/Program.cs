using SeeSharp.Experiments;
using SeeSharp.Image;

namespace MisForCorrelatedBidir.BidirExperiment {
    class Program {

        static void RunBench(int splitfactor) {
            int resolutionScale = 2;
            SceneRegistry.AddSource("../Scenes");

            Benchmark bench = new(new BidirExperiment(), new() {
                SceneRegistry.LoadScene("ModernHall"),
                SceneRegistry.LoadScene("LivingRoom"),
                SceneRegistry.LoadScene("TargetPractice"),
                SceneRegistry.LoadScene("HomeOffice"),
                SceneRegistry.LoadScene("RoughGlasses"),
                SceneRegistry.LoadScene("RoughGlassesIndirect"),
                SceneRegistry.LoadScene("IndirectRoom"),
                SceneRegistry.LoadScene("MinimalistWhiteRoom"),
                SceneRegistry.LoadScene("LampCaustic"),
            }, $"Results-x{splitfactor}", 640 * resolutionScale, 480 * resolutionScale,
            FrameBuffer.Flags.SendToTev);

            bench.Run(format: ".exr");
        }

        static void Main(string[] args) {
            RunBench(10);
            RunBench(50);
            RunBench(100);
        }
    }
}
